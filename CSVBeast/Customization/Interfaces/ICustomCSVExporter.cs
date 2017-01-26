using System;
using System.Collections.Generic;
using Astronautics.ABMS.Common.CSVExport.CSVTable.Interfaces;
using Astronautics.ABMS.Common.CSVExport.Errata;

namespace Astronautics.ABMS.Common.CSVExport.Customization.Interfaces
{
    public interface ICustomCSVExporter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnInfo"></param>
        /// <param name="row"></param>
        /// <param name="item"></param>
        void ExportToCSVTable(CSVTable.CSVTable table, ICSVColumn columnInfo, ICSVRow row, object item);

        /// <summary>
        /// Imports value from CSV table string to target object
        /// </summary>
        /// <param name="csvTable"></param>
        /// <param name="currentRowIndex"></param>
        /// <param name="columnInfo"></param>
        /// <param name="targetType"></param>
        /// <param name="convertedValue"></param>
        /// <param name="errors"></param>
        /// <returns>Number of ADDITIONAL rows that were consumed during import of the data member</returns>
        int ImportMemberFromCSV(CSVTable.CSVTable csvTable, int currentRowIndex, ICSVColumn columnInfo, Type targetType, out object convertedValue, ICollection<ICSVImportErrorInfo> errors);
    }
}
