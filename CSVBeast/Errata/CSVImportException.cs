using System;
using System.Runtime.Serialization;

namespace KotturTech.CSVBeast.Errata
{
    [Serializable]
    public class CSVImportException : Exception, ICSVImportErrorInfo
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public CSVImportException()
        {
        }

        public CSVImportException(string message)
            : base(message)
        {
        }

        public CSVImportException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CSVImportException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Error Severity
        /// </summary>
        public CSVImportErrorSeverity Severity { get; set; }
        
        /// <summary>
        /// Index of row in CSV file, where the error occurred
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Exception that caused the error
        /// </summary>
        public Exception Exception
        {
            get { return this; }
        }
    }
}
