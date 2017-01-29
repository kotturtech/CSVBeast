using System;

namespace KotturTech.CSVBeast.CSVDataBuilder
{
    /// <summary>
    /// Describes class member types to export
    /// </summary>
    [Flags]
    public enum CSVExportTargets
    {
        NotSet = 0,
        Properties = 2,
        Fields = 4
    }

    /// <summary>
    /// When set on a class or a struct, this attribute indicates that all class members (Whether it is fields, properties or both)
    /// should be exported to CSV
    /// </summary>
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
