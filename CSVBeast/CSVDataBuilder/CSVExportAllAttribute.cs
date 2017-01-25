using System;

namespace CSVBeast.CSVDataBuilder
{
    [Flags]
    public enum CSVExportTargets
    {
        NotSet = 0,
        Properties = 2,
        Fields = 4
    }

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public class CSVExportAllAttribute : Attribute
    {
        public CSVExportAllAttribute() 
        {
            ExportTargets = CSVExportTargets.Properties;
        }

        public CSVExportTargets ExportTargets { get; set; }
    }
}
