using System;

namespace KotturTech.CSVBeast.Errata
{
    /// <summary>
    /// Interface for description of errors generated on CSV import
    /// </summary>
    public interface ICSVImportErrorInfo
    {
        /// <summary>
        /// Error severity
        /// </summary>
        CSVImportErrorSeverity Severity { get; }
        
        /// <summary>
        /// Error message
        /// </summary>
        string Message { get; }
        
        /// <summary>
        /// Index of row in CSV file, at which the error occurred
        /// </summary>
        int RowIndex { get; }

        /// <summary>
        /// Exception that caused the error
        /// </summary>
        Exception Exception { get; }
    }
}
