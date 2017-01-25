using CSVBeast.CSVDataBuilder;
using CSVBeast.Errata;

namespace CSVBeast
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Windows;


    public class CSVExporter
    {

        #region Nested Types


        public enum TargetFileExistsBehavior
        {
            OverwriteWithPrompt,
            OverwriteSilent,
            HaltWithError
        }

        public enum FileOverwriteBehavior
        {
            Truncate,
            Append
        }

        #endregion

        #region Private Fields

        private FileMode _fileCreationMode;
        private bool _appending;
        private EventHandler<ProgressChangedEventArgs> _progressChanged;
        private EventHandler<CSVImportExportErrorEventArgs> _errorOccurred;

        #endregion

        #region Constructor

        public CSVExporter(string fileNameAndPath)
        {
            FileNameAndPath = fileNameAndPath;
            BehaviorOnTargetFileExists = TargetFileExistsBehavior.OverwriteWithPrompt;
            CharacterEncoding = Encoding.ASCII;
            BehaviorOnFileOverwrite = FileOverwriteBehavior.Truncate;
            ExportTargets = CSVExportTargets.Properties;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Specifies the target file name, including the full path
        /// </summary>
        public string FileNameAndPath { get; private set; }

        /// <summary>
        /// Specifies behavior when target file already exists
        /// </summary>
        public TargetFileExistsBehavior BehaviorOnTargetFileExists { get; set; }

        /// <summary>
        /// Specifies behavior in case when file needs to be overwritten - Whether new file will be created, or new data will be appended to new data
        /// </summary>
        public FileOverwriteBehavior BehaviorOnFileOverwrite { get; set; }

        /// <summary>
        /// Specifies the character encoding for output file
        /// </summary>
        public Encoding CharacterEncoding { get; set; }

        /// <summary>
        /// Sets the action that prompts whether file should be overwritten or not.
        /// Called only in case when BehaviorOnTargetFileExists = OverwriteWithPrompt.
        /// This delegate can be used for displaying custom styled dialog as a prompt.
        /// </summary>
        public Func<bool> OverridePromptFileOverwrite;


        /// <summary>
        /// Specifies which members to export by default, in case no attributes set on target type
        /// </summary>
        public CSVExportTargets ExportTargets { get; set; }


        /// <summary>
        /// Event that indicates that progress of the export was changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged
        {
            add { _progressChanged += value; }
            remove { if (_progressChanged != null) _progressChanged -= value; }
        }

        /// <summary>
        /// Event that indicates that an error was occurred during export
        /// </summary>
        public event EventHandler<CSVImportExportErrorEventArgs> ErrorOccurred
        {
            add { _errorOccurred += value; }
            remove { if (_errorOccurred != null) _errorOccurred -= value; }
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Exports the data collection to CSV file
        /// </summary>
        /// <param name="data">Data for export</param>
        public void ExportData(IEnumerable data)
        {

            FileStream fileStream = null;
            StreamWriter fileStreamWriter = null;
            try
            {
                GetFileOpeningParams();
                CSVTable.CSVTable table;
                //In case user chose to append files - First import the previous table
                if (_appending)
                {
                    var importer = new CSVImporter(FileNameAndPath) { CharacterEncoding = CharacterEncoding };
                    importer.ProgressChanged += (e, a) =>
                    {
                        if (_progressChanged != null)
                        {
                            _progressChanged.Invoke(this, new ProgressChangedEventArgs(a.ProgressPercentage, "Loading table from old file..."));
                        }
                    };
                    importer.ErrorOccurred += (e, a) =>
                    {
                        if (_errorOccurred != null)
                        {
                            _errorOccurred.Invoke(this, a);
                        }
                    };
                    table = importer.ImportCSVTableFromFile();
                }
                else
                {
                    table = new CSVTable.CSVTable();
                }

                fileStream = File.Open(FileNameAndPath, _fileCreationMode, FileAccess.Write);
                fileStreamWriter = new StreamWriter(fileStream, CharacterEncoding);

                var builder = new CSVDataBuilder.CSVDataBuilder {ExportTargets = ExportTargets};
                builder.BuildCSVTable(data, table);
                fileStreamWriter.WriteLine(table.GetCSVHeader());
                var rowcount = table.RowCount;
                for (var i = 0; i < rowcount; i++)
                {
                    string csvRow;
                    table.GetCSVRow(i, out csvRow);
                    fileStreamWriter.WriteLine(csvRow);
                    fileStreamWriter.Flush();
                    if (_progressChanged != null)
                    {
                        var percents = (int)(((double)i) / (rowcount - 1) * 100.0);
                        _progressChanged.Invoke(this,
                            new ProgressChangedEventArgs(percents, "Exporting Dataset"));
                    }
                }


            }
            catch (Exception e)
            {
                if (_errorOccurred != null)
                    _errorOccurred.Invoke(this, new CSVImportExportErrorEventArgs(e));
                else throw;
            }
            finally
            {
                if (fileStreamWriter != null)
                {
                    fileStreamWriter.Close();
                    fileStreamWriter.Dispose();
                }
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }

        }

        #endregion

        #region Private Methods

        private void GetFileOpeningParams()
        {
            _fileCreationMode = FileMode.CreateNew;
            _appending = false;
            switch (BehaviorOnTargetFileExists)
            {
                case TargetFileExistsBehavior.HaltWithError:
                    {
                        break;
                    }
                case TargetFileExistsBehavior.OverwriteSilent:
                    {
                        if (BehaviorOnFileOverwrite == FileOverwriteBehavior.Append)
                            _fileCreationMode = FileMode.Append;
                        else if (BehaviorOnFileOverwrite == FileOverwriteBehavior.Truncate)
                            _fileCreationMode = FileMode.OpenOrCreate;
                        else
                            throw new InvalidEnumArgumentException(
                                @"Unsupported or undefined value of FileOverwriteBehavior enum encountered");
                        break;
                    }
                case TargetFileExistsBehavior.OverwriteWithPrompt:
                    {
                        if (File.Exists(FileNameAndPath))
                        {
                            bool overwrite = true;
                            if (OverridePromptFileOverwrite == null)
                            {
                                var res = MessageBox.Show(
                                    string.Format("File: {0} already exists! {1} Do you want to replace the old file?",
                                        FileNameAndPath,
                                        Environment.NewLine), "", MessageBoxButton.YesNo);
                                if (res == MessageBoxResult.No)
                                    overwrite = false;
                            }
                            else
                            {
                                overwrite = OverridePromptFileOverwrite.Invoke();
                            }
                            if (overwrite)
                            {
                                if (BehaviorOnFileOverwrite == FileOverwriteBehavior.Append)
                                {
                                    _fileCreationMode = FileMode.Truncate; //Not FileMode.Append; We load the CSV table from old file first, join the new data to it, and save it into new file to get valid CSV file
                                    _appending = true;
                                }
                                else if (BehaviorOnFileOverwrite == FileOverwriteBehavior.Truncate)
                                    _fileCreationMode = FileMode.Truncate;
                                else
                                    throw new InvalidEnumArgumentException(
                                        @"Unsupported or undefined value of FileOverwriteBehavior enum encountered");
                            }
                            else
                            {
                                _fileCreationMode = FileMode.CreateNew;
                            }

                        }
                        else
                        {
                            _fileCreationMode = FileMode.CreateNew;
                        }
                        break;
                    }
                default:
                    {
                        throw new InvalidEnumArgumentException(
                            @"Unsupported or undefined value of TargetFileExistsBehavior enum encountered");
                    }
            }
        }
        #endregion
    }


}
