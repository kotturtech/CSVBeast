using System;

namespace Astronautics.ABMS.Common.CSVExport.CSVDataBuilder
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CSVExportIgnoreAttribute : Attribute
    {

    }
}
