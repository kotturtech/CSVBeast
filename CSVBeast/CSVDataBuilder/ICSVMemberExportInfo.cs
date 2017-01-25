using System;

namespace CSVBeast.CSVDataBuilder
{
    internal interface ICSVMemberExportInfo
    {
        string ColumnName { get;  }
        int SortOrder { get; }
        bool SkipOnImport { get; }
        Type CustomExporterType { get; }
    }
}
