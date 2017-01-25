using System;

namespace CSVBeast.CSVTable.Interfaces
{

    /// <summary>
    /// Interface for CSV column for use with CSVTable object
    /// </summary>
    public interface ICSVColumn : IComparable<ICSVColumn>
    {
        /// <summary>
        /// The column name in CSV table
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// Column order in which it is added to CSV file
        /// </summary>
        int SortOrder { get; }
    }
}
