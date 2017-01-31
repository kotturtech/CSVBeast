using System;
using System.Collections.Generic;
using System.Linq;
using KotturTech.CSVBeast.CSVTable.Interfaces;
using KotturTech.CSVBeast.Customization.Interfaces;
using KotturTech.CSVBeast.Errata;

namespace KotturTech.CSVBeast.Customization.Implementations
{
    /// <summary>
    /// Default importer - Implements default logic for converting data of CSV column into class member type
    /// </summary>
    public class DefaultCSVExporter : ICustomCSVExporter
    {
        public void ExportToCSVTable(CSVTable.CSVTable targetTable, ICSVColumn columnInfo, ICSVRow row, object item)
        {
             targetTable.AddColumn(columnInfo);
             row.SetValue(columnInfo.ColumnName, item);
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

            //Case 2: IConvertible, attempt to use it if applicable. Will work for primitives
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

            //Case 3: If the type is simply assignable from the source type
            if (targetType.IsAssignableFrom(columnValue.GetType())) //If value from column can be just assigned to target type = leave things as they are
            {
                convertedValue = columnValue;
                return 0;
            }

            //No method could convert? Report error!
            errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Error,
                    string.Format("Column: {0} contains value: {1}, which can't be converted to target value of type: {2}", columnInfo.ColumnName, columnValue, targetType.FullName),
                    currentRowIndex+1));

            convertedValue = null;
            return 0;

        }
    }
}
