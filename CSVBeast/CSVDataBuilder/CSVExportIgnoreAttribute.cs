using System;

namespace CSVBeast.CSVDataBuilder
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CSVExportIgnoreAttribute : Attribute
    {

    }
}
