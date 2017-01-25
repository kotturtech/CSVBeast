using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using CSVBeast.CSVDataBuilder;
using CSVBeast.CSVTable;
using CSVBeast.Customization.Interfaces;
using CSVBeast.Errata;

namespace CSVBeast
{
    public class CSVImporter
    {

        #region Private Fields

        private FileStream _fileStream;
        private StreamReader _fileStreamReader;
        private bool _disposed;
        private EventHandler<ProgressChangedEventArgs> _progressChanged;
        private EventHandler<CSVImportExportErrorEventArgs> _errorOccurred;
        private const string FormatErrorString =
            "File {0} is corrupt, or not a valid CSV file, error occurred while processing line {1}";
        private Encoding _characterEncoding;
        private int _newlineCharsLength;

        #endregion

        #region Public Properties

        /// <summary>
        /// Specifies the source file name, including the full path
        /// </summary>
        public string FileNameAndPath { get; private set; }

        /// <summary>
        /// Specifies the character encoding for input file
        /// </summary>
        public Encoding CharacterEncoding
        {
            get { return _characterEncoding; }
            set
            {
                if (value == null)
                    throw new Exception("CharacterEncoding cannot be null");
                _characterEncoding = value;
                _newlineCharsLength = _characterEncoding.GetBytes(Environment.NewLine).Length;
            }
        }

        /// <summary>
        /// Specifies which members to export by default, in case no attributes set on target type
        /// </summary>
        public CSVExportTargets ExportTargets { get; set; }

        /// <summary>
        /// Event that indicates that progress of table loading was changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged
        {
            add
            {
                _progressChanged += value;
            }
            remove
            {
                if (_progressChanged != null) _progressChanged -= value;
            }
        }

        /// <summary>
        /// Event that indicates that an error was occurred during CSV table loading
        /// </summary>
        public event EventHandler<CSVImportExportErrorEventArgs> ErrorOccurred
        {
            add
            {
                _errorOccurred += value;
            }
            remove
            {
                if (_errorOccurred != null) _errorOccurred -= value;
            }
        }


        #endregion

        #region Constructor

        public CSVImporter(string fileNameAndPath)
        {
            FileNameAndPath = fileNameAndPath;
            CharacterEncoding = Encoding.ASCII;
            ExportTargets = CSVExportTargets.Properties;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Imports dataset
        /// </summary>
        /// <typeparam name="T">Base type of dataset items</typeparam>
        /// <param name="dataset">Collection that will contain imported items</param>
        /// <param name="typeFactory">Type factory for creation of objects according to values in imported row</param>
        public void ImportToDataSet<T>(ICollection<T> dataset,ICSVImportObjectFactory<T> typeFactory = null)
        {
            var table = ImportCSVTableFromFile();
            var builder = new CSVDataBuilder.CSVDataBuilder { ExportTargets = ExportTargets }; 
            IEnumerable<ICSVImportErrorInfo> errors;
            builder.BuildTypeDataSet(table, dataset, out errors,typeFactory);
            var csvImportErrorInfos = errors as ICSVImportErrorInfo[] ?? errors.ToArray();
            if (csvImportErrorInfos.Any() && _errorOccurred != null)
                _errorOccurred.Invoke(this, new CSVImportExportErrorEventArgs(csvImportErrorInfos));
        }

        /// <summary>
        /// Loads CSV table from file. File name is set by FileNameAndPath property
        /// </summary>
        /// <returns>Resulting loaded table</returns>
        public CSVTable.CSVTable ImportCSVTableFromFile()
        {
            if (_disposed)
                return null;

            CSVTable.CSVTable table = null;

            try
            {
                var lineCounter = 1;
                long bytesRead = 0;

                //Opening source file
                _fileStream = File.Open(FileNameAndPath, FileMode.Open, FileAccess.Read);
                _fileStreamReader = new StreamReader(_fileStream, CharacterEncoding);
                var filesize = new FileInfo(FileNameAndPath).Length;

                //Creating table and header
                table = new CSVTable.CSVTable();
                var header = _fileStreamReader.ReadLine();

                if (string.IsNullOrEmpty(header))
                    throw new FileFormatException(string.Format(FormatErrorString, FileNameAndPath, lineCounter));
                var splitHeader = header.Split(',');
                var columnCounter = 0;
                table.AddColumns(from column in splitHeader select new CSVColumn(column, ++columnCounter));

                bytesRead += _characterEncoding.GetBytes(header).Length + _newlineCharsLength;

                //Filling the rows
                while (!_fileStreamReader.EndOfStream)
                {
                    //Getting the line and verifying that it is valid
                    lineCounter++;
                    var row = _fileStreamReader.ReadLine();

                    if (string.IsNullOrEmpty(row))
                        throw new FileFormatException(string.Format(FormatErrorString, FileNameAndPath, lineCounter));
                    var splitRow = row.Split(',');
                    if (splitRow.Length != splitHeader.Length)
                        throw new FileFormatException(string.Format(FormatErrorString, FileNameAndPath, lineCounter));

                    bytesRead += _characterEncoding.GetBytes(row).Length + _newlineCharsLength;

                    //Adding the new row to table and filling it
                    var newRow = table.AddRow();

                    for (var i = 0; i < columnCounter; i++)
                        newRow.SetValue(splitHeader[i], splitRow[i]);

                    //Reporting progress
                    if (_progressChanged != null)
                    {
                        var percents = (double)bytesRead / filesize * 100.0;
                        _progressChanged.Invoke(this, new ProgressChangedEventArgs((int)percents, "Loading CSV File...."));
                    }
                }
            }
            catch (Exception e)
            {
                if (_disposed) //If object was disposed = Dont care about the exception
                    return null;

                if (_errorOccurred != null)
                    _errorOccurred.Invoke(this, new CSVImportExportErrorEventArgs(e));
                else throw;
            }
            finally
            {
                Cleanup();
            }

            return table;
        }

        #endregion

  
        #region Private Methods
        private void Cleanup()
        {
            if (_fileStreamReader != null)
            {
                _fileStreamReader.Dispose();
                _fileStreamReader = null;
            }

            if (_fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }
        }

        #endregion

        #region Disposable Pattern

        ~CSVImporter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;

            Cleanup();

            if (disposing)
                GC.SuppressFinalize(this);

        }

        #endregion

    }
}
