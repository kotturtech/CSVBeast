using System;
using System.Collections.Generic;

namespace CSVBeast.Errata
{
    public class CSVImportExportErrorEventArgs : EventArgs
    {
        public CSVImportExportErrorEventArgs(IEnumerable<ICSVImportErrorInfo> errors)
        {
            ImportErrors = errors;
            Exception = null;
        }

        public CSVImportExportErrorEventArgs(Exception e)
        {
            ImportErrors = new List<ICSVImportErrorInfo>();
            Exception = e;
        }

        public Exception Exception { get; private set; }
        public IEnumerable<ICSVImportErrorInfo> ImportErrors { get; private set; }
    }
}
