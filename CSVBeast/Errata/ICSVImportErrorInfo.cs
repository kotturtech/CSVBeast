using System;

namespace CSVBeast.Errata
{
    public interface ICSVImportErrorInfo
    {
        CSVImportErrorSeverity Severity { get; }
        string Message { get; }
        int RowIndex { get; }
        Exception Exception { get; }
    }
}
