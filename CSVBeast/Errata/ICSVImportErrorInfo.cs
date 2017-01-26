using System;

namespace Astronautics.ABMS.Common.CSVExport.Errata
{
    public interface ICSVImportErrorInfo
    {
        CSVImportErrorSeverity Severity { get; }
        string Message { get; }
        int RowIndex { get; }
        Exception Exception { get; }
    }
}
