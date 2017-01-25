using CSVBeast.CSVTable.Interfaces;

namespace CSVBeast.Customization.Interfaces
{
    /// <summary>
    /// Interface for a factory for creating imported objects during import of CSV data
    /// </summary>
    /// <typeparam name="T">Base type of dataset item</typeparam>
    public interface ICSVImportObjectFactory<T>
    {
        /// <summary>
        /// Creates an object based on data in CSV row
        /// </summary>
        /// <param name="iCSVRow">Source CSV row</param>
        /// <param name="targetObject">Created empty object that should be popultated by csv data</param>
        void CreateObject(ICSVRow iCSVRow, out T targetObject);
    }
}
