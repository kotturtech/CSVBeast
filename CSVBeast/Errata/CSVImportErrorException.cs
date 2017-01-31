using System;
using System.Runtime.Serialization;

namespace Astronautics.ABMS.Common.CSVExport.Errata
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
            RowIndex = -1;
        }

        public CSVImportException(string message)
            : base(message)
        {
            RowIndex = -1;
        }

        public CSVImportException(string message, Exception inner)
            : base(message, inner)
        {
            RowIndex = -1;
        }

        protected CSVImportException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            RowIndex = -1;
        }

        public CSVImportErrorSeverity Severity { get; set; }
        public int RowIndex { get; set; }
        public Exception Exception
        {
            get { return this; }
        }
    }
}
