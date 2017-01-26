using System;

namespace Astronautics.ABMS.Common.CSVExport.CSVDataBuilder
{
    internal interface ICSVMemberExportInfo
    {
        string ColumnName { get; }
        int SortOrder { get; }
        bool SkipOnImport { get; }
        Type CustomExporterType { get; }
    }
}
