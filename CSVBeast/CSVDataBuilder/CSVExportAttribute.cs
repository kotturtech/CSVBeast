using System;
using System.Linq;
using KotturTech.CSVBeast.Customization.Interfaces;

namespace KotturTech.CSVBeast.CSVDataBuilder
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CSVExportAttribute : Attribute, ICSVMemberExportInfo
    {
        private Type _customExporterType;

        #region Public Properties

        public string ColumnName { get; private set; }
        public int SortOrder { get; set; }
        public bool SkipOnImport { get; set; }

        public Type CustomExporterType
        {
            get { return _customExporterType; }
            set
            {
                if (value.GetInterfaces().Contains(typeof(ICustomCSVExporter)))
                    _customExporterType = value;
                else
                {
                    throw new ArgumentException("The specified type must implement ICustomCSVExporter interface");
                }
            }
        }

        #endregion

        #region Constructor

        public CSVExportAttribute(string column)
        {
            if (string.IsNullOrWhiteSpace(column))
                throw new ArgumentException("Column name cannot be empty or null");
            if (column == "ID")
                throw new ArgumentException("The value [ID] as CSV column name may cause problem to open resulting file in MS EXCEL. Please try something else, like [Id]");
            if (column.Contains(','))
                throw new ArgumentException(
                    string.Format("The column name [{0}] contains comma, export will result in undefined CSV table",
                        ColumnName));
            ColumnName = column;
        }

        #endregion

    }
}
