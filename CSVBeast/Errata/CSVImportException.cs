using System;
using System.Runtime.Serialization;

namespace CSVBeast.Errata
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

        public CSVImportErrorSeverity Severity { get; set; }
        public int RowIndex { get; set; }
        public Exception Exception
        {
            get { return this; }
        }
    }
}
