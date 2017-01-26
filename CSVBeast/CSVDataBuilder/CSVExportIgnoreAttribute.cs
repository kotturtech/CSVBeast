using System;

namespace KotturTech.CSVBeast.CSVDataBuilder
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CSVExportIgnoreAttribute : Attribute
    {

    }
}
