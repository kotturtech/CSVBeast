using System;
using System.Collections.Generic;

namespace Astronautics.ABMS.Common.CSVExport.Errata
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
