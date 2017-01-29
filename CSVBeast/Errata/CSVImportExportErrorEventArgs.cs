using System;
using System.Collections.Generic;

namespace KotturTech.CSVBeast.Errata
{
    /// <summary>
    /// Event args for event that raised in case an error occures
    /// </summary>
    public class CSVImportExportErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Instantiates a class
        /// </summary>
        /// <param name="errors">Collection of Error objects</param>
        public CSVImportExportErrorEventArgs(IEnumerable<ICSVImportErrorInfo> errors)
        {
            ImportErrors = errors;
            Exception = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">Exception that caused the error</param>
        public CSVImportExportErrorEventArgs(Exception e)
        {
            ImportErrors = new List<ICSVImportErrorInfo>();
            Exception = e;
        }

        /// <summary>
        /// Exception that caused the error
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Error list
        /// </summary>
        public IEnumerable<ICSVImportErrorInfo> ImportErrors { get; private set; }
    }
}
