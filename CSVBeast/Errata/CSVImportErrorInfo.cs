using System;

namespace KotturTech.CSVBeast.Errata
{
    public class CSVImportErrorInfo : ICSVImportErrorInfo
    {

        public CSVImportErrorInfo(CSVImportErrorSeverity severity, string userMessage, int rowIndex, Exception exception = null)
        {
            Severity = severity;
            Message = userMessage;
            RowIndex = rowIndex;
            Exception = exception;
        }

        /// <summary>
        /// Severity of an error
        /// </summary>
        public CSVImportErrorSeverity Severity { get; protected set; }
        
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; protected set; }
        
        /// <summary>
        /// Index of CSV row at which the error occurred
        /// </summary>
        public int RowIndex { get; protected set; }

        /// <summary>
        /// Exception that caused the error
        /// </summary>
        public Exception Exception { get; protected set; }
    }
}
