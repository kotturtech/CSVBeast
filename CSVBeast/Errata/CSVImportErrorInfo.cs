using System;

namespace Astronautics.ABMS.Common.CSVExport.Errata
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

        public CSVImportErrorSeverity Severity { get; protected set; }
        public string Message { get; protected set; }
        public int RowIndex { get; protected set; }
        public Exception Exception { get; protected set; }
    }
}
