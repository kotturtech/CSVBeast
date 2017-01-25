using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CSVBeast.CSVTable.Interfaces;
using CSVBeast.Customization.Interfaces;
using CSVBeast.Errata;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using CSVBeast.Customization.Implementations;

namespace CSVBeast.CSVDataBuilder
{
    public class CSVDataBuilder
    {
        #region Nested Types

        #region Export Attribute Handling

        private abstract class CSVExportMemberInfo : ICSVColumn
        {
            public int SortOrder { get; private set; }
            public string ColumnName { get; private set; }
            public bool SkipOnImport { get; private set; }
            private readonly ICustomCSVExporter _valueExporter;
            private readonly IEnumerable<ValidationAttribute> _validationAttributes;
            

            protected CSVExportMemberInfo(ICSVMemberExportInfo exportData, IEnumerable<ValidationAttribute> validationAttributes)
            {
                SortOrder = exportData.SortOrder;
                ColumnName = exportData.ColumnName;
                SkipOnImport = exportData.SkipOnImport;
                _validationAttributes = validationAttributes;
                if (exportData.CustomExporterType != null)
                {
                    _valueExporter = (ICustomCSVExporter)Activator.CreateInstance(exportData.CustomExporterType);
                }
                else
                    _valueExporter = new DefaultCSVExporter();
            }

            protected abstract string MemberName { get; }
            protected abstract Type TargetType { get; }
            protected abstract object ExtractValue(object source);
            protected abstract void AssignValue(object target, object value);

            public int CompareTo(ICSVColumn other)
            {
                var sortOrderComp = SortOrder.CompareTo(other.SortOrder);
                return sortOrderComp != 0 ? sortOrderComp : String.Compare(ColumnName, other.ColumnName, StringComparison.Ordinal);
            }

            public void ExportToTable(object item, ICSVRow row, CSVTable.CSVTable table)
            {
                _valueExporter.ExportToCSVTable(table, this, row, ExtractValue(item));
            }

            public int ImportFromTable(CSVTable.CSVTable csvTable, int currentRowIndex, object targetObject, ICollection<ICSVImportErrorInfo> errors)
            {
                if (SkipOnImport)
                    return 0;

                object importedMemberValue;
                var errorCount = errors.Count;
                var consumedRows = 0;

                //Get the converted value for assigning
                try
                {
                    consumedRows = _valueExporter.ImportMemberFromCSV(csvTable, currentRowIndex, this, TargetType, out importedMemberValue, errors);
                    if (errorCount < errors.Count) //Errors were added
                        return consumedRows;
                }
                catch (CSVImportException e)
                {
                    e.RowIndex = currentRowIndex;
                    errors.Add(e);
                    return consumedRows;
                }
                catch (Exception e)
                {
                    errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Fatal,
                        string.Format("Value exporter of type {0} failed to import value for class member: {1}, CSV Column Name: {2}", _valueExporter.GetType().FullName, MemberName, ColumnName),
                        currentRowIndex, e));
                    return consumedRows;
                }

                //Validate the value with validation attributes
                try
                {
                    foreach (var validationAttribute in _validationAttributes)
                    {
                        if (validationAttribute.IsValid(importedMemberValue))
                            continue;
                        var result = validationAttribute.GetValidationResult(importedMemberValue,
                            new ValidationContext(importedMemberValue));
                        errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Error,
                            string.Format("Column {0} contains invalid value. Value is {1} Validation error is: {2}",
                                ColumnName, importedMemberValue, result != null ? result.ErrorMessage : "Unknown Error"),
                            currentRowIndex));
                    }
                }
                catch (CSVImportException e)
                {
                    e.RowIndex = currentRowIndex;
                    errors.Add(e);
                    return consumedRows;
                }
                catch (Exception e)
                {
                    errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Fatal,
                        string.Format("Validation process failed for column value: {0}, class member {1} ", ColumnName, MemberName),
                        currentRowIndex, e));
                    return consumedRows;
                }

                //Assign the value to member
                try
                {
                    AssignValue(targetObject, importedMemberValue);
                }
                catch (CSVImportException e)
                {
                    e.RowIndex = currentRowIndex;
                    errors.Add(e);
                }
                catch (Exception e)
                {
                    errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Fatal,
                        string.Format("Coulnd't assign value {0} to class member: {1}", importedMemberValue, MemberName),
                        currentRowIndex, e));
                }

                return consumedRows;
            }
        }

        private class PropertyCSVExportMemberInfo : CSVExportMemberInfo
        {
            private readonly PropertyInfo _propertyInfo;
            private readonly Type _targetType;
            private readonly string _memberName;

            public PropertyCSVExportMemberInfo(ICSVMemberExportInfo exportData, IEnumerable<ValidationAttribute> validationAttributes, PropertyInfo info)
                : base(exportData, validationAttributes)
            {
                _propertyInfo = info;
                _targetType = _propertyInfo.PropertyType;
                _memberName = _propertyInfo.Name;
            }

            protected override object ExtractValue(object source)
            {
                if (source == null)
                    return null;
                return _propertyInfo.GetValue(source, null);
            }

            protected override void AssignValue(object target, object value)
            {
                if (!_propertyInfo.CanWrite)
                    throw new CSVImportException(
                        string.Format("Property {0} is a read-only property, can't assign value to it",_propertyInfo.Name))
                    {
                        Severity = CSVImportErrorSeverity.Debug
                    };
                _propertyInfo.SetValue(target, value);
            }

            protected override Type TargetType
            {
                get { return _targetType; }
            }

            protected override string MemberName
            {
                get { return _memberName; }
            }
        }

        private class FieldCSVExportMemberInfo : CSVExportMemberInfo
        {
            private readonly FieldInfo _fieldInfo;
            private readonly Type _targetType;
            private readonly string _memberName;

            public FieldCSVExportMemberInfo(ICSVMemberExportInfo exportData, IEnumerable<ValidationAttribute> validationAttributes, FieldInfo info)
                : base(exportData, validationAttributes)
            {
                _fieldInfo = info;
                _targetType = _fieldInfo.FieldType;
                _memberName = _fieldInfo.Name;
            }

            protected override object ExtractValue(object source)
            {
                if (source == null)
                    return null;
                return _fieldInfo.GetValue(source);
            }

            protected override void AssignValue(object target, object value)
            {
                _fieldInfo.SetValue(target, value);
            }

            protected override Type TargetType
            {
                get { return _targetType; }
            }

            protected override string MemberName
            {
                get { return _memberName; }
            }
        }

        private class TypeCSVExporter
        {
            private readonly SortedSet<CSVExportMemberInfo> _exportedMemberInfo = new SortedSet<CSVExportMemberInfo>();

            public void AddExportedMemberInfo(CSVExportMemberInfo info)
            {
                _exportedMemberInfo.Add(info);
            }

            public void ExportToTable(object item, CSVTable.CSVTable table)
            {

                //Create Header
                table.AddColumns(_exportedMemberInfo);
                var row = table.AddRow();
                foreach (var memberToExport in _exportedMemberInfo)
                {
                    memberToExport.ExportToTable(item, row, table);
                }
            }

            public int FillObject(CSVTable.CSVTable sourceTable, int currentRowIndex, object targetObject, ICollection<ICSVImportErrorInfo> errors)
            {
                var consumedRows = 1;
                foreach (var memberToImport in _exportedMemberInfo)
                {
                    //The ImportFromTable method will return quantity of ADDITIONALY consumed rows
                    consumedRows += memberToImport.ImportFromTable(sourceTable, currentRowIndex, targetObject, errors);
                }
                return consumedRows;
            }
        }

        #endregion

        #region Default Interface Implementations

        private class DefaultCSVExporter : ICustomCSVExporter
        {
            public void ExportToCSVTable(CSVTable.CSVTable targetTable, ICSVColumn columnInfo, ICSVRow row, object item)
            {
                if (item is IEnumerable && typeof(string) != item.GetType())
                {
                    var listExporter = new ComponentListCSVExporter();
                    listExporter.ExportToCSVTable(targetTable, columnInfo, row, item);
                }
                else
                {
                    targetTable.AddColumn(columnInfo);
                    row.SetValue(columnInfo.ColumnName, item);
                }
            }

            public int ImportMemberFromCSV(CSVTable.CSVTable csvTable, int currentRowIndex, ICSVColumn columnInfo, Type targetType, out object convertedValue, ICollection<ICSVImportErrorInfo> errors)
            {
                //Get relevant column value from the table
                var row = csvTable[currentRowIndex];
                object columnValue;
                if (!row.GetValue(columnInfo.ColumnName, out columnValue))
                {
                    errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Error,
                        string.Format("CSV table doesn't contain the requested column: {0}", columnInfo.ColumnName),
                        currentRowIndex));
                    convertedValue = null;
                    return 0;
                }

                //Things get tricky now - Need to find a way to convert the source object to target type correctly

                //Case 1: In case the value is null
                var notString = targetType != typeof(string);
                if (columnValue == null || (notString && string.IsNullOrWhiteSpace(columnValue.ToString()))) //If the column value is null - check if null can be assigned to target member
                {
                    if (targetType.IsValueType)
                    {
                        errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Error,
                        string.Format("Column {0} contains null value, which can't be assigned to property or field of type: {1}", columnInfo.ColumnName, targetType.FullName),
                        currentRowIndex));
                        convertedValue = null;
                        return 0;
                    }
                    convertedValue = null;
                    return 0;
                }

                //Case 2: Type has TypeConverterAttribute
                var typeConverterAttribute =
                    targetType.GetCustomAttributes(typeof(System.ComponentModel.TypeConverterAttribute), true).FirstOrDefault() as System.ComponentModel.TypeConverterAttribute;
                if (typeConverterAttribute != null)
                {
                    try
                    {
                        var converterType = Type.GetType(typeConverterAttribute.ConverterTypeName, false, true);
                        TypeConverter converter;
                        if (converterType == typeof(EnumConverter) || converterType.BaseType == typeof(EnumConverter))
                        {
                            //In case of enum converter it is known that it has constructor that gets type as parameter
                            converter = Activator.CreateInstance(converterType, new object[] { targetType }) as TypeConverter;
                        }
                        else
                        {
                            converter = Activator.CreateInstance(converterType) as TypeConverter;
                        }

                        if (converter != null)
                        {
                            convertedValue = converter.ConvertFromString(columnValue.ToString());
                            return 0;
                        }
                    }
                    catch (Exception)
                    {
                        //Ignore
                    }
                }

                //Case 3: IEnumerable, since string is also IEnumerable we exclude the case of string here
                if (notString && targetType.GetInterfaces().Any(x => x.Name.Contains("IEnumerable")))
                {
                    var listExporter = new ComponentListCSVExporter();
                    return listExporter.ImportMemberFromCSV(csvTable, currentRowIndex, columnInfo, targetType,
                        out convertedValue, errors);
                }

                //Case 4: IConvertible, attempt to use it if applicable
                if (targetType.GetInterfaces().Any(x => x == typeof(IConvertible)))
                {
                    try
                    {
                        convertedValue = Convert.ChangeType(columnValue, targetType);
                        return 0;
                    }
                    catch (Exception e)
                    {
                        errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Error,
                        string.Format("Column {0} contains value: {1}, which can't be converted to target value of type: {2}", columnInfo.ColumnName, columnValue, targetType.FullName),
                        currentRowIndex, e));
                        convertedValue = null;
                        return 0;
                    }
                }

                //Case 5: If the type is simply assignable from the source type
                if (targetType.IsAssignableFrom(columnValue.GetType())) //If value from column can be just assigned to target type = leave things as they are
                {
                    convertedValue = columnValue;
                    return 0;
                }

                //Case 6: Wild guess - Try to retrieve Parse method and use it
                var parseMethod = targetType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
                if (parseMethod != null)
                {
                    try
                    {
                        convertedValue = parseMethod.Invoke(null, new object[] { columnValue.ToString() });
                        return 0;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                //No method could convert? Report error!
                errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Error,
                        string.Format("Column: {0} contains value: {1}, which can't be converted to target value of type: {2}", columnInfo.ColumnName, columnValue, targetType.FullName),
                        currentRowIndex));

                convertedValue = null;
                return 0;

            }
        }

        private class DefaultImportTypeFactory<T> : ICSVImportObjectFactory<T>
        {
            public void CreateObject(ICSVRow iCSVRow, out T targetObject)
            {
                targetObject = (T)Activator.CreateInstance(typeof(T));
            }
        }

        private class CSVMemberExportInfo : ICSVMemberExportInfo
        {
            public CSVMemberExportInfo(bool skipOnImport)
            {
                SkipOnImport = skipOnImport;
                CustomExporterType = null;
            }

            public string ColumnName { get; set; }
            public int SortOrder { get; set; }
            public bool SkipOnImport { get; private set; }
            public Type CustomExporterType { get; private set; }
            
        }

        #endregion


        #endregion

        #region Constructor

        public CSVDataBuilder()
        {
            ExportTargets = CSVExportTargets.Properties;
        }

        #endregion

        #region Private Fields

        private readonly Dictionary<string, TypeCSVExporter> _typeExportersDictionary = new Dictionary<string, TypeCSVExporter>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Specifies which members to export by default, in case no attributes set on target type
        /// </summary>
        public CSVExportTargets ExportTargets { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Constructs CSV table out of collection of objects, according to class members that were marked by CSVExportAttribute.
        /// The resulting table is an union of those attributes.
        /// </summary>
        /// <param name="sourceCollection">The source dataset to build CSV table from</param>
        /// <param name="table">The CSV table object to fill</param>
        public void BuildCSVTable(IEnumerable sourceCollection,CSVTable.CSVTable table)
        {
            foreach (var item in sourceCollection)
            {
                var exporter = GetExporterForType(item.GetType());
                exporter.ExportToTable(item, table);
            }
        }


        /// <summary>
        /// Construct classes from CSV table
        /// </summary>
        /// <typeparam name="T">The base type of the target dataset</typeparam>
        /// <param name="sourceTable">CSV table to import from</param>
        /// <param name="dataset">A collection to fill</param>
        /// <param name="errors">A collection of error data during import</param>
        /// <param name="typeFactory">Type factory, that generates types in cases when dataset is a polymorphic collection</param>
        public void BuildTypeDataSet<T>(CSVTable.CSVTable sourceTable, ICollection<T> dataset, out IEnumerable<ICSVImportErrorInfo> errors, ICSVImportObjectFactory<T> typeFactory = null)
        {
            ICollection<ICSVImportErrorInfo> errorsCollection = new List<ICSVImportErrorInfo>();
            if (typeFactory == null)
                typeFactory = new DefaultImportTypeFactory<T>();

            var currentRowIndex = 0;
            while (currentRowIndex < sourceTable.RowCount)
            {
                T targetObject;
                typeFactory.CreateObject(sourceTable[currentRowIndex], out targetObject);
                var exporter = GetExporterForType(targetObject.GetType());
                currentRowIndex += exporter.FillObject(sourceTable, currentRowIndex, targetObject, errorsCollection);
                dataset.Add(targetObject);
            }
            errors = errorsCollection;
        }

        #endregion

        #region Private Methods

        private TypeCSVExporter GetExporterForType(Type type)
        {
            if (_typeExportersDictionary.ContainsKey(type.FullName))
                return _typeExportersDictionary[type.FullName];

            var typeExportInfo = new TypeCSVExporter();
            var objectExportSettings = (type.GetCustomAttributes(typeof (CSVExportAllAttribute), true).FirstOrDefault() as CSVExportAllAttribute);
            bool shouldExportProperties = (ExportTargets & CSVExportTargets.Properties) > 0, shouldExportFields = (ExportTargets & CSVExportTargets.Fields) > 0;
            if (objectExportSettings != null)
            {
                shouldExportFields = (objectExportSettings.ExportTargets & CSVExportTargets.Fields) > 0;
                shouldExportProperties = (objectExportSettings.ExportTargets & CSVExportTargets.Properties) > 0;
            }
            int memberCounter = 0;
            var props = type.GetProperties();
            var fields = type.GetFields();
            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes(typeof(CSVExportAttribute), true);
                var validationAttrs = prop.GetCustomAttributes(typeof(ValidationAttribute), true);
                if (attrs.Any())
                {
                    typeExportInfo.AddExportedMemberInfo(
                        new PropertyCSVExportMemberInfo((CSVExportAttribute) attrs.First(),
                            validationAttrs.OfType<ValidationAttribute>(), prop));
                }
                else
                {
                    if (shouldExportProperties && !(prop.GetCustomAttributes(typeof (CSVExportIgnoreAttribute), true).Any() || prop.GetCustomAttributes(typeof (XmlIgnoreAttribute), true).Any()))
                    {
                        var info = new CSVMemberExportInfo(!prop.CanWrite)
                        {
                            ColumnName = prop.Name,
                            SortOrder = memberCounter++
                        };
                         typeExportInfo.AddExportedMemberInfo(
                         new PropertyCSVExportMemberInfo(info,validationAttrs.OfType<ValidationAttribute>(), prop));
                    }
                }
            }
            foreach (var field in fields)
            {
                var attrs = field.GetCustomAttributes(typeof(CSVExportAttribute), true);
                var validationAttrs = field.GetCustomAttributes(typeof(ValidationAttribute), true);
                if (attrs.Any())
                    typeExportInfo.AddExportedMemberInfo(new FieldCSVExportMemberInfo((CSVExportAttribute)attrs.First(), validationAttrs.OfType<ValidationAttribute>(), field));
                else
                {
                    if (shouldExportFields && !(field.GetCustomAttributes(typeof (CSVExportIgnoreAttribute), true).Any() || field.GetCustomAttributes(typeof (XmlIgnoreAttribute), true).Any()))
                    {
                        var info = new CSVMemberExportInfo(false)
                        {
                            ColumnName = field.Name,
                            SortOrder = memberCounter++
                        };
                         typeExportInfo.AddExportedMemberInfo(
                         new FieldCSVExportMemberInfo(info,validationAttrs.OfType<ValidationAttribute>(), field));
                    }
                }
            }


            _typeExportersDictionary.Add(type.FullName, typeExportInfo);
            return typeExportInfo;
        }

        #endregion

    }
}
