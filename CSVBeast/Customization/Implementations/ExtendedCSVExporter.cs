using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using KotturTech.CSVBeast.Customization.Interfaces;
using KotturTech.CSVBeast.Errata;

namespace KotturTech.CSVBeast.Customization.Implementations
{
    public class ExtendedCSVExporter : ICustomCSVExporter
    {
        private readonly ComponentListCSVExporter _componentListExporter;
        private readonly DefaultCSVExporter _defaultCSVExporter;
        
        public ExtendedCSVExporter()
        {
            _componentListExporter = new ComponentListCSVExporter();
            _defaultCSVExporter = new DefaultCSVExporter();
        }

        public virtual void ExportToCSVTable(CSVTable.CSVTable table, CSVTable.Interfaces.ICSVColumn columnInfo, CSVTable.Interfaces.ICSVRow row, object item)
        {
            if (item is IEnumerable && typeof (string) != item.GetType())
            {
                _componentListExporter.ExportToCSVTable(table, columnInfo, row, item);
            }
            else
            {
                _defaultCSVExporter.ExportToCSVTable(table,columnInfo,row,item);
            }
        }

        public virtual int ImportMemberFromCSV(CSVTable.CSVTable csvTable, int currentRowIndex, CSVTable.Interfaces.ICSVColumn columnInfo, Type targetType, out object convertedValue, ICollection<ICSVImportErrorInfo> errors)
        {

            var row = csvTable[currentRowIndex];
            object columnValue;
            if (!row.GetValue(columnInfo.ColumnName, out columnValue))
            {
                errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Error,
                    string.Format("CSV table doesn't contain the requested column: {0}", columnInfo.ColumnName),
                    currentRowIndex+1));
                convertedValue = null;
                return 0;
            }

            //Case 1: Attempt to use the default
            var defaultErrorList = new List<ICSVImportErrorInfo>();
            var def = _defaultCSVExporter.ImportMemberFromCSV(csvTable, currentRowIndex, columnInfo, targetType, out convertedValue, defaultErrorList);
            if (!defaultErrorList.Any())
                return def;

            //Case 2: IEnumerable, since string is also IEnumerable we exclude the case of string here
            var notString = targetType != typeof(string);
            if (notString && targetType.GetInterfaces().Any(x => x.Name.Contains("IEnumerable")))
            {
                return _componentListExporter.ImportMemberFromCSV(csvTable, currentRowIndex, columnInfo, targetType,
                    out convertedValue, errors);
            }

            //Case 3: Type has TypeConverterAttribute
            var typeConverterAttribute =
                targetType.GetCustomAttributes(typeof(TypeConverterAttribute), true).FirstOrDefault() as TypeConverterAttribute;
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

            //Case 4: Wild guess - Try to retrieve Parse method and use it
            if (!targetType.IsPrimitive)
            {
                var parseMethod = targetType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
                if (parseMethod != null)
                {
                    try
                    {
                        convertedValue = parseMethod.Invoke(null, new object[] {columnValue.ToString()});
                        return 0;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            //Point of no hope, appending errors and returning
            foreach (var errorInfo in defaultErrorList)
                errors.Add(errorInfo);

            return 0;
        }
    }
}
