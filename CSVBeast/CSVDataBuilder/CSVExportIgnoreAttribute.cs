using System;

namespace KotturTech.CSVBeast.CSVDataBuilder
{
    /// <summary>
    /// When set on field or property, this attribute indicates that this member or field should be ignored by CSV import/export engine.
    /// This attribute is useful in conjunction with ExportAll attribute, to exclude specific fields
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CSVExportIgnoreAttribute : Attribute
    {

    }
}
