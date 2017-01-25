using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSVBeast.CSVTable.Interfaces;

namespace CSVBeast.CSVTable
{
    public class CSVTable
    {
        #region Nested Types

        private class HeaderChangedArgs : EventArgs
        {

            #region Nested Types

            public enum ChangeType
            {
                Add,
                Remove
            };

            #endregion

            #region Public Properties

            public ICSVColumn Item { get; private set; }
            public ChangeType HeaderChangeType { get; private set; }

            #endregion

            #region Constructor

            public HeaderChangedArgs(ICSVColumn item, ChangeType change)
            {
                HeaderChangeType = change;
                Item = item;
            }

            #endregion
        }

        private class CSVHeader
        {

            #region Private Fields

            private readonly SortedSet<ICSVColumn> _items = new SortedSet<ICSVColumn>();
            private EventHandler<HeaderChangedArgs> _headerChanged;

            #endregion

            #region Public Properties

            public event EventHandler<HeaderChangedArgs> HeaderChanged
            {
                add
                {
                    _headerChanged += value;
                }
                remove { if (_headerChanged != null) _headerChanged -= value; }
            }

            #endregion

            #region Public Methods

            public void AddColumns(IEnumerable<ICSVColumn> columns)
            {
                foreach (var column in columns)
                {

                    if (_items.Any(x => x.ColumnName == column.ColumnName)) //Making sure that column heading is unique
                        return;
                    _items.Add(column);
                    if (_headerChanged != null)
                        _headerChanged.Invoke(this, new HeaderChangedArgs(column, HeaderChangedArgs.ChangeType.Add));
                }
            }

            public void AddColumn(ICSVColumn column)
            {

                if (_items.Any(x => x.ColumnName == column.ColumnName)) //Making sure that column heading is unique
                    return;
                _items.Add(column);
                if (_headerChanged != null)
                    _headerChanged.Invoke(this, new HeaderChangedArgs(column, HeaderChangedArgs.ChangeType.Add));

            }

            public void RemoveColumn(string columnName)
            {
                var key = _items.FirstOrDefault(x => x.ColumnName == columnName);
                if (key == null)
                    return;
                _items.Remove(key);
                if (_headerChanged != null)
                    _headerChanged.Invoke(this, new HeaderChangedArgs(key, HeaderChangedArgs.ChangeType.Remove));
            }

            public IEnumerable<ICSVColumn> GetColumns()
            {
                return _items;
            }

            #region Overrides of Object

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (var key in _items)
                {
                    sb.Append(key.ColumnName);
                    sb.Append(CommaSeparator);
                }
                sb.Remove(sb.Length - 1, 1);
                return sb.ToString();
            }

            #endregion

            #endregion

        }

        private class CSVRow : ICSVRow
        {
            #region Private Fields

            private readonly SortedDictionary<ICSVColumn, object> _content = new SortedDictionary<ICSVColumn, object>();
            private readonly CSVHeader _header;

            #endregion

            #region Constructor

            public CSVRow(CSVHeader header)
            {
                _header = header;
                var headerItems = header.GetColumns();
                foreach (var headerItem in headerItems)
                    _content.Add(headerItem, null);
                _header.HeaderChanged += OnHeaderChanged;
            }

            #endregion

            #region Public Methods

            public void SetValue(string heading, object value)
            {
                var key = _content.Keys.FirstOrDefault(x => x.ColumnName.Equals(heading));
                if (key == null) //No column with such header? Add it!
                {
                    var max = _content.Any() ? _content.Keys.Max(x => x.SortOrder) : 1;
                    _header.AddColumn(new CSVColumn(heading, max + 1)); //Adding column to row will do the job for the entire table
                    key = _content.Keys.First(x => x.ColumnName.Equals(heading)); //At the end of addition, the key with required criteria will certainly exist
                }

                _content[key] = value;

            }

            public bool GetValue(string columnName, out object value)
            {
                var key = _content.Keys.FirstOrDefault(x => x.ColumnName.Equals(columnName));
                var noSuchColumn = key == null;
                value = noSuchColumn ? null : _content[key];
                return !noSuchColumn;
            }

            public SortedDictionary<ICSVColumn, object> GetData()
            {
                var data = new SortedDictionary<ICSVColumn, object>();
                foreach (var item in _content)
                    data.Add(item.Key, item.Value);
                return data;
            }

            #region Overrides of Object

            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var item in _content)
                {
                    sb.Append(item.Value ?? "");
                    sb.Append(CommaSeparator);
                }
                sb.Remove(sb.Length - 1, 1);
                return sb.ToString();
            }

            #endregion

            #endregion

            #region Event Handlers

            private void OnHeaderChanged(object sender, HeaderChangedArgs e)
            {
                if (e.HeaderChangeType == HeaderChangedArgs.ChangeType.Add)
                    _content.Add(e.Item, null);
                if (e.HeaderChangeType == HeaderChangedArgs.ChangeType.Remove)
                    _content.Remove(e.Item);
            }

            #endregion

        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The number of rows in CSV table, excluding the header
        /// </summary>
        public int RowCount { get { return _rows.Count; } }

        /// <summary>
        /// The collection of rows contained in CSV table
        /// </summary>
        public IEnumerable<ICSVRow> Rows { get { return _rows; } }

        /// <summary>
        /// Gets the table row at specified index
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Table row at specified index</returns>
        public ICSVRow this[int i]
        {
            get { return _rows[i]; }
        }


        #endregion

        #region Private Fields

        private readonly CSVHeader _header = new CSVHeader();
        private readonly List<CSVRow> _rows = new List<CSVRow>();
        private const char CommaSeparator = ',';

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds columns to header of the CSV table header
        /// </summary>
        /// <param name="columns">Collection of columns to add</param>
        public void AddColumns(IEnumerable<ICSVColumn> columns)
        {
            _header.AddColumns(columns);
        }

        /// <summary>
        /// Adds a single column to CSV table header
        /// </summary>
        /// <param name="columnInfo">Column to add to header</param>
        public void AddColumn(ICSVColumn columnInfo)
        {
            _header.AddColumn(columnInfo);
        }

        /// <summary>
        /// Removes the entire column from CSV table
        /// </summary>
        /// <param name="columnName">Column name to remove</param>
        public void RemoveColumn(string columnName)
        {
            _header.RemoveColumn(columnName);
        }

        /// <summary>
        /// Adds row to the CSV table. If the row data contains new columns, they will be added as well. 
        /// </summary>
        /// <param name="rowData">A dictionary with key that indicates a column, and value is the value matching to the column. Null value will result in adding an empty row</param>
        /// <returns>The index of the added row</returns>
        public int AddRow(IDictionary<string, object> rowData)
        {
            var row = new CSVRow(_header);
            int nextIndex = _rows.Count;
            _rows.Add(row);
            if (rowData == null)
                return nextIndex;
            foreach (var rowItem in rowData)
            {
                row.SetValue(rowItem.Key, rowItem.Value);
            }
            return nextIndex;
        }

        /// <summary>
        /// Adds an empty row to CSV table
        /// </summary>
        /// <returns>The newly created row</returns>
        public ICSVRow AddRow()
        {
            var row = new CSVRow(_header);
            _rows.Add(row);
            return row;
        }

        /// <summary>
        /// Add a row to CSV table
        /// </summary>
        /// <param name="csvRow">Row to add</param>
        public void AddRow(ICSVRow csvRow)
        {
            var row = new CSVRow(_header);
            var data = csvRow.GetData();
            foreach (var item in data)
                row.SetValue(item.Key.ColumnName, item.Value);
            _rows.Add(row);
        }

        /// <summary>
        /// Returns the CSV table header as string
        /// </summary>
        /// <returns></returns>
        public string GetCSVHeader()
        {
            return _header.ToString();
        }

        /// <summary>
        /// Returns the row of CSV table at specified index, as string
        /// </summary>
        /// <param name="index">Index of the row</param>
        /// <param name="rowData">The requested row, as string</param>
        public void GetCSVRow(int index, out string rowData)
        {
            var row = _rows[index];
            rowData = row.ToString();
        }

        #endregion
    }
}
