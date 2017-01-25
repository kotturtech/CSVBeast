using System;
using CSVBeast.CSVTable.Interfaces;

namespace CSVBeast.CSVTable
{
    /// <summary>
    /// Class that encapsulates a CSV column and allows sorting the columns according to user specified order
    /// </summary>
    public class CSVColumn : ICSVColumn
    {

        #region Public Properties

        /// <summary>
        /// CSV Column Name
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Order of adding the column to CSV file
        /// </summary>
        public int SortOrder { get; private set; }

        #endregion

        #region Constructor

        public CSVColumn(string heading, int sortOrder = 0)
        {
            ColumnName = heading;
            SortOrder = sortOrder;
        }

        #endregion

        #region Implementation of IComparable<HeaderItem>

        /// <summary>
        /// Compares two CSV columns by order of adding to CSV table. The comparision logic is to sort first by SortOrder property, and
        /// in case of a tie use alphabetical order of column name
        /// </summary>
        /// <param name="other">object to compare to</param>
        /// <returns>comparision result</returns>
        public int CompareTo(ICSVColumn other)
        {
            var sortOrderComp = SortOrder.CompareTo(other.SortOrder);
            return sortOrderComp != 0 ? sortOrderComp : string.Compare(ColumnName, other.ColumnName, StringComparison.Ordinal);
        }

        #endregion

    }
}
