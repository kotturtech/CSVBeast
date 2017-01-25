using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSVBeast.CSVTable.Interfaces;
using CSVBeast.Customization.Interfaces;
using CSVBeast.Errata;

namespace CSVBeast.Customization.Implementations
{
    /// <summary>
    /// ComponentListCSVExporter used for exporting members of classes - collections of components.
    /// This custom exporter creates another CSV table out of the collection and appends it to the
    /// constructed table.
    /// </summary>
    public class ComponentListCSVExporter : ICustomCSVExporter
    {

        protected virtual object ComponentTypeFactory { get; set; }

        public virtual void ExportToCSVTable(CSVTable.CSVTable table, ICSVColumn columnInfo, ICSVRow row, object item)
        {
           
            //Checking null value of property
            if (item == null)
            {
                row.SetValue(columnInfo.ColumnName, 0);
                return;
            }

            //Checking that the type of the member is as expected
            var items = item as IEnumerable;
            if (items == null)
                throw new ArgumentException("ComponentListCSVExporter is not used correctly - You can use it on members of type: IEnumerable only");

            //Creating CSV table from member collection
            var itemsCSVTable = new CSVTable.CSVTable();
            CSVDataBuilder.CSVDataBuilder builder = new CSVDataBuilder.CSVDataBuilder();
            builder.BuildCSVTable(items, itemsCSVTable);
            int counter = 0;
            //Appending the rows from resulting table to the target table
            foreach (var csvRow in itemsCSVTable.Rows)
            {
                table.AddRow(csvRow);
                counter++;
            }

            //Setting the count of items in the original row
            row.SetValue(columnInfo.ColumnName, counter);
        }


        public virtual int ImportMemberFromCSV(CSVTable.CSVTable csvTable, int currentRowIndex, ICSVColumn columnInfo,
            Type targetType, out object convertedValue,
            ICollection<ICSVImportErrorInfo> errors)
        {
            object rowCountString;
            if (!csvTable[currentRowIndex].GetValue(columnInfo.ColumnName, out rowCountString))
            {
                errors.Add(new CSVImportErrorInfo(CSVImportErrorSeverity.Fatal,
                    string.Format("Couldn't retrieve value for column: {0}", columnInfo.ColumnName), currentRowIndex));
                convertedValue = targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                return 0;
            }

            int consumedRows = int.Parse(rowCountString.ToString());

            //Construct subtable of rows
            CSVTable.CSVTable subtable = new CSVTable.CSVTable();
            var dataStartIndex = currentRowIndex + 1;
            var maxIndex = dataStartIndex + consumedRows;
            if (maxIndex > csvTable.RowCount)
            {
                throw new CSVImportException(
                    string.Format(
                        "Corrupt value of column {0}, could cause an attempt to read more lines than table actually contains",
                        columnInfo.ColumnName))
                {
                    RowIndex = currentRowIndex
                };
            }

            for (var i = currentRowIndex + 1; i < maxIndex; i++)
                subtable.AddRow(csvTable[i]);

            //Use the importer to create dataset - The target types need to be retrieved using reflection
            Type elementType = GetElementType(targetType);
            CSVDataBuilder.CSVDataBuilder builder = new CSVDataBuilder.CSVDataBuilder();
            Type dataSetType = typeof(List<>);
            dataSetType = dataSetType.MakeGenericType(elementType);
            var dataSet = Activator.CreateInstance(dataSetType);
            var buildMethod = builder.GetType()
                .GetMethod("BuildTypeDataSet", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance);
            buildMethod = buildMethod.MakeGenericMethod(elementType);
            
            object[] args = new[] {subtable, dataSet, null, ComponentTypeFactory};
            buildMethod.Invoke(builder, args);
            ICollection<ICSVImportErrorInfo> objErrors = args[2] as ICollection<ICSVImportErrorInfo>; //Getting the Out parameter
            if (objErrors != null)
            {
                foreach (var error in objErrors)
                {
                    errors.Add(new CSVImportErrorInfo(error.Severity,string.Format("Member Dataset import error: {0}",error.Message),error.RowIndex + currentRowIndex,error.Exception));
                }
            }


            //convert dataset to something that matches the target value
            if (targetType.IsArray)
            {
                var arr = Array.CreateInstance(elementType, consumedRows);
                ((ICollection)dataSet).CopyTo(arr, 0);
                convertedValue = arr;
            }
            else if (targetType.IsInterface)
                //Since GetElementType already validated the input type, we can be sure that dataset implements required interfaces
                convertedValue = dataSet;
            else
            {
                convertedValue = Activator.CreateInstance(targetType);
                foreach (var item in (IEnumerable)dataSet)
                {
                    targetType.InvokeMember("Add",
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, convertedValue,
                        new[] { item });
                }
            }

            return consumedRows;
        }


        protected static Type GetElementType(Type targetType)
        {
            if (targetType.IsArray)
                return targetType.GetElementType();
            var genericParams = targetType.GenericTypeArguments;
            var interfaces = targetType.GetInterfaces();
            foreach (var genericParam in genericParams)
            {
                Type ienumType = typeof(IEnumerable<>);
                ienumType = ienumType.MakeGenericType(genericParam);
                if (interfaces.Any(x => x == ienumType))
                    return genericParam;
            }

            throw new Exception(string.Format("Unexpected member target type: {0}, Generic IEnumerable or Array type is expected", targetType));
        }

    }
}
