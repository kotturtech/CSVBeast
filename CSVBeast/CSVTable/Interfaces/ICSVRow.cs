using System.Collections.Generic;

namespace KotturTech.CSVBeast.CSVTable.Interfaces
{
    public interface ICSVRow
    {
        /// <summary>
        /// Sets value for specified column in CSV row
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Value to set</param>
        void SetValue(string columnName, object value);

        /// <summary>
        /// Gets value for specified column in CSV row
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <param name="value">The resulting value. If column doesn't exist in the row - null will be assigned</param>
        /// <returns>bool - indicated whether the row contained the specified column</returns>
        bool GetValue(string columnName, out object value);

        /// <summary>
        /// Gets the data of the CSV row as dictionary
        /// </summary>
        /// <returns></returns>
        SortedDictionary<ICSVColumn, object> GetData();
    }

}
