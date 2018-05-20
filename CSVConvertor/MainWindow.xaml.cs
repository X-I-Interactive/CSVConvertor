using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using CSVConvertor.Domain;
using CsvHelper;
using System.Collections.ObjectModel;
using Convertor.Respository.DataLayer;
using Convertor.Respository.ConvertorClasses;
using CSVConvertor.Processes;

namespace CSVConvertor

// to do:
//  needs replacement values added
//  what should defaults be
//  
//  inclusion/exclusion field
//  confirmation of field headings and sample row
//  Company name/scheme code needs converting to type, value pair for use in defaults
// error log
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private String OriginalFileName = String.Empty;
        public string CSVAppFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "RobertSmithManagement", "CSVConvertor");

        ObservableCollection<ListBoxPair> MappingFiles = new ObservableCollection<ListBoxPair>();

        private MappingDefinition CurrentMappingDefinition = new MappingDefinition();
        private string CurrentMappingFilename = string.Empty;
        private FileTypeForLoading FileTypeForLoading { get; set; }
        private List<MiddlewareField> DefinitionFields = new List<MiddlewareField>();
        private List<MiddlewareField> RequiredDefinitionFields = new List<MiddlewareField>();
        private List<MappingFilter> MappingFilters = new List<MappingFilter>();

        private List<List<string>> CSVDataIn = new List<List<string>>();
        private List<List<string>> CSVDataOut = new List<List<string>>();

        CSVDataProcesses CSVDataProcesses = new CSVDataProcesses();

        // additional file status
        public ObservableCollection<AdditionalFileStatus> AdditionalFileStatusList = new ObservableCollection<AdditionalFileStatus>();

        public MainWindow()
        {
            InitializeComponent();

            InitialiseScreenComponents();

        }

        #region "Screen controls"

        private void ExitForm_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        

        private void MappingFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MappingFileList.SelectedIndex >= 1)
            {
                CurrentMappingFilename = ((ListBoxPair)MappingFileList.SelectedItem).Value;

                CurrentMappingDefinition = CSVConvertorFileManagement.ReadMappingFile(CurrentMappingFilename, CSVAppFolder, ".xml");
                if (CurrentMappingDefinition.HasAdditionalFile)
                {
                    SetAdditionFileDisplay(CurrentMappingDefinition);
                    AdditionalFilePanel.Visibility = Visibility.Visible;
                }
                else
                {
                    AdditionalFilePanel.Visibility = Visibility.Hidden;
                    //textBlockAdditionalFileStatus.Text = string.Empty;
                }

            }
        }

        private void LoadLevel2File_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            fileDialog.DefaultExt = ".csv";
            fileDialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";
            fileDialog.CheckFileExists = true;
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            Nullable<bool> result = fileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                UpdateLevel2File(fileDialog.FileName);
            }
        }

        private void SelectOriginalFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            fileDialog.DefaultExt = ".csv";
            fileDialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";
            fileDialog.CheckFileExists = true;
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            Nullable<bool> result = fileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                OriginalFileName = fileDialog.SafeFileName;
                OriginalFile.Text = fileDialog.FileName;
            }
        }

        private void SetDestination_Click(object sender, RoutedEventArgs ev)
        {
            DestinationLocation.Text = GetFolder();
        }


        private void ConvertFile_Click(object sender, RoutedEventArgs ev)
        {
            //  check everything is ready, file to process, destination and mapping file
            if (OriginalFileName == string.Empty || !File.Exists(OriginalFile.Text.Trim()))
            {
                MessageBox.Show("Please select a file to process");
                return;
            }

            if (MappingFileList.SelectedIndex < 1)
            {
                MessageBox.Show("Please select a mapping");
                return;
            }

            string temp = DestinationLocation.Text.Trim();
            if (temp == string.Empty || !Directory.Exists(temp))
            {
                MessageBox.Show("Please select a destination location");
                return;
            }

            CurrentMappingDefinition = CSVConvertorFileManagement.ReadMappingFile(((ListBoxPair)MappingFileList.SelectedItem).Value, CSVAppFolder, ".xml");

            // finally, check mapping file for level 2 records
            if (CurrentMappingDefinition.RequiresLevel2File && CurrentMappingDefinition.Level2Records.Count() == 0)
            {
                MessageBox.Show("There are no level 2 records loaded");
                return;
            }

            int rowsProcessed = ConvertCSVFile(OriginalFile.Text.Trim(), FirstRowHeadersCheckBox.IsChecked ?? false);
            if (rowsProcessed < 0)
            {
                return;
            }
            {
                MessageBox.Show("Processing complete, rows output: " + rowsProcessed);
            }

        }

        private void optionButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow optionsWindow = new OptionsWindow(CSVAppFolder);

            optionsWindow.Owner = App.Current.MainWindow;

            optionsWindow.ShowDialog();

        }

        private void ManageMappingsButton_Click(object sender, RoutedEventArgs e)
        {
            MappingWindow mappingWindow = new MappingWindow(CSVAppFolder);

            mappingWindow.Owner = App.Current.MainWindow;
            mappingWindow.ShowDialog();

            InitialiseScreenComponents();

        }

        private void OpenManagementSettings_Click(object sender, RoutedEventArgs e)
        {
            CheckPasswordOpenBox();
        }

        private void CheckPasswordOpenBox()
        {
            if (passwordBox.Password.Trim() == Properties.Settings.Default.ManagementAccess)
            {
                ManagementPanel.Visibility = Visibility.Visible;
            }
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckPasswordOpenBox();
            }
        }

        private void ImportMappingsFile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(CSVConvertorFileManagement.ImportMappingFile(CSVAppFolder));
        }

        private void ImportITMFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(CSVConvertorFileManagement.ImportDefinitionFile(CSVAppFolder));
            InitialiseScreenComponents();

        }

        private void fileRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // ... Get RadioButton reference.
            var radioButton = sender as RadioButton;
            selectedFileType.Text = radioButton.Content.ToString();
            loadSelectedFile.IsEnabled = false;
            switch (radioButton.Name)
            {
                case "level2RadioButton":
                    FileTypeForLoading = FileTypeForLoading.Level2;
                    break;
                case "optoutChangesRadioButton":
                    FileTypeForLoading = FileTypeForLoading.OptOutChanges;
                    break;
                case "optoutNewRadioButton":
                    FileTypeForLoading = FileTypeForLoading.OptOutNew;
                    break;
                case "errorsRadioButton":
                    FileTypeForLoading = FileTypeForLoading.Errors;
                    break;
                default:
                    FileTypeForLoading = FileTypeForLoading.None;
                    return;
            }
            loadSelectedFile.IsEnabled = true;
        }

        private void loadSelectedFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            fileDialog.DefaultExt = ".csv";
            fileDialog.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt";
            fileDialog.CheckFileExists = true;
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            Nullable<bool> result = fileDialog.ShowDialog();

            if (!result.HasValue || !result.Value)
            {
                MessageBox.Show("No file loaded");
                return;
            }

            string fileName = fileDialog.FileName;

            switch (FileTypeForLoading)
            {
                case FileTypeForLoading.Level2:
                    UpdateLevel2File(fileName);
                    break;
                case FileTypeForLoading.OptOutNew:
                    UpdateOptoutFile(fileName, true);
                    break;
                case FileTypeForLoading.OptOutChanges:
                    UpdateOptoutFile(fileName, false);
                    break;
                case FileTypeForLoading.Errors:
                    UpdateErrorFile(fileName);
                    break;
                case FileTypeForLoading.None:
                    return;
            }

            // save updated mapping file and update screen
            CSVConvertorFileManagement.SaveMappingFile(CurrentMappingDefinition, CurrentMappingFilename, CSVAppFolder, ".xml");
            SetAdditionFileDisplay(CurrentMappingDefinition);
        }
        #endregion

        #region "Helper fuunctions"

        private void InitialiseScreenComponents()
        {
            CSVConvertorFileManagement.CreateWorkingFolder(CSVAppFolder);

            MappingFiles = new ObservableCollection<ListBoxPair>();
            MappingFiles.Add(new ListBoxPair { Key = "0", Value = "(Select a mapping)" });

            foreach (var file in CSVConvertorFileManagement.GetMappingFiles(CSVAppFolder, ".xml"))
            {
                MappingFiles.Add(new ListBoxPair(file, file));
            }
            MappingFileList.DisplayMemberPath = "Value";
            MappingFileList.SelectedValuePath = "Key";
            MappingFileList.ItemsSource = MappingFiles;

            MappingFileList.SelectedIndex = 0;

            ManagementPanel.Visibility = Visibility.Hidden;
            AdditionalFilePanel.Visibility = Visibility.Hidden;

            loadSelectedFile.IsEnabled = false;
        }

        private static string GetFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }

            return string.Empty;
        }

        private void UpdateLevel2File(string level2File)
        {
            List<List<string>> level2Data = CSVConvertorFileManagement.LoadCSVFile(level2File, true);

            if (level2Data == null)
            {
                MessageBox.Show("Error reading level 2 file");
                return;
            }
            CurrentMappingDefinition.Level2Records = level2Data;
            CurrentMappingDefinition.DateLevel2LastLoaded = DateTime.Now;
            
            return;

            // 
        }

        private void UpdateOptoutFile(string optOutFile, bool replaceFile)
        {
            var optOutRecords = File.ReadAllLines(optOutFile)
                                            .Skip(1)
                                            .Select(v => OptOutRecord.FromCSV(v))
                                            .ToList();
            if (replaceFile)
            {
                CurrentMappingDefinition.OptOutRecords = optOutRecords;
                CurrentMappingDefinition.DateOptOutRecordsLastLoaded = DateTime.Now;
                return;
            }

            CurrentMappingDefinition.OptOutRecords.AddRange(optOutRecords);
            CurrentMappingDefinition.DateOptOutRecordsLastLoaded = DateTime.Now;
        }

        private void UpdateErrorFile(string errorFile)
        {
            var errorRecords = File.ReadAllLines(errorFile)
                                            .Skip(1)
                                            .Select(v => FileErrorRecord.FromCSV(v))
                                            .ToList();

            CurrentMappingDefinition.FileErrorRecords.AddRange(errorRecords);
            CurrentMappingDefinition.DateErrorRecordsLastLoaded = DateTime.Now;
        }

        private void SetAdditionFileDisplay(MappingDefinition currentMappingDefinition)
        {
            loadSelectedFile.IsEnabled = false;
            additionalFileStatusTable.DataContext = null;
            AdditionalFileStatusList = new ObservableCollection<AdditionalFileStatus>();
            var radioButtons = additionalFiletypeGroup.Children.OfType<RadioButton>();
            foreach(var rButton in radioButtons)
            {
                rButton.IsChecked = false;
                rButton.IsEnabled = false;
            }
            //textBlockAdditionalFileStatus.Text = string.Format("Level 2 records: {0}", currentMappingDefinition.Level2Records.Count());
            selectedFileType.Text = "None";

            errorsRadioButton.IsEnabled = currentMappingDefinition.CanHaveErrorFile;
            level2RadioButton.IsEnabled = currentMappingDefinition.RequiresLevel2File;
            optoutChangesRadioButton.IsEnabled = currentMappingDefinition.RequiresOptOutFile;
            optoutNewRadioButton.IsEnabled = currentMappingDefinition.RequiresOptOutFile;

            AdditionalFileStatus additionalFileStatus = new AdditionalFileStatus();

            if (currentMappingDefinition.RequiresLevel2File)
            {
                level2RadioButton.IsEnabled = true;
                additionalFileStatus = new AdditionalFileStatus();
                additionalFileStatus.FileType = "Level 2";
                additionalFileStatus.RowCount = currentMappingDefinition.Level2Records.Count();
                additionalFileStatus.DateLastUpdated = currentMappingDefinition.DateLevel2LastLoaded == DateTime.MinValue?"-": currentMappingDefinition.DateLevel2LastLoaded.ToShortDateString();
                AdditionalFileStatusList.Add(additionalFileStatus);
            }

            if (currentMappingDefinition.RequiresOptOutFile)
            {
                optoutNewRadioButton.IsEnabled = true;
                additionalFileStatus = new AdditionalFileStatus();
                additionalFileStatus.FileType = "Opt out file";
                additionalFileStatus.RowCount = currentMappingDefinition.OptOutRecords.Count();
                additionalFileStatus.DateLastUpdated = currentMappingDefinition.DateOptOutRecordsLastLoaded == DateTime.MinValue ? "-" : currentMappingDefinition.DateOptOutRecordsLastLoaded.ToShortDateString();
                AdditionalFileStatusList.Add(additionalFileStatus);
            }

            if (currentMappingDefinition.CanHaveErrorFile)
            {
                errorsRadioButton.IsEnabled = true;
                additionalFileStatus = new AdditionalFileStatus();
                additionalFileStatus.FileType = "Errors file";
                additionalFileStatus.RowCount = currentMappingDefinition.FileErrorRecords.Count();
                additionalFileStatus.DateLastUpdated = currentMappingDefinition.DateErrorRecordsLastLoaded == DateTime.MinValue ? "-" : currentMappingDefinition.DateErrorRecordsLastLoaded.ToShortDateString();
                AdditionalFileStatusList.Add(additionalFileStatus);
            }

            additionalFileStatusTable.AutoGenerateColumns = true;
            additionalFileStatusTable.ItemsSource = AdditionalFileStatusList;
            
        }

        private int ConvertCSVFile(string csvFileName, bool firstRowHasHeaders)
        {

            // initialise
            DefinitionFields = CSVConvertorFileManagement.ReadDefinitionFile(CurrentMappingDefinition.DefinitionFile, CSVAppFolder, ".xml");
            MappingFilters = CurrentMappingDefinition.MappingFilters;

            // load CSV file
            if (!LoadCSVPayrollFile(csvFileName, firstRowHasHeaders))
            {
                return -1;
            }

            CSVDataOut = new List<List<string>>();

            MakeHeaderRow();
            string excludeField = Properties.Settings.Default.Level2ExcludeField;
            MakeCSVBody(excludeField);

            return WriteCSVFile();
        }

        private int WriteCSVFile()
        {
            string fileName = System.IO.Path.Combine(DestinationLocation.Text.Trim(), "Processed_" + OriginalFileName);
            StreamWriter textWriter = new StreamWriter(fileName);
            int rowsProcessed = 0;

            foreach (var item in CSVDataOut)
            {
                textWriter.WriteLine(string.Join(",", item));
                rowsProcessed++;
            }

            if (CurrentMappingDefinition.RequiresLevel2File)
            {
                foreach(var item in CurrentMappingDefinition.Level2Records)
                {
                    //  need to add a check for embedded commas !!!!!!!!!!!!!!!!!!
                    textWriter.WriteLine(string.Join(",", item));
                    rowsProcessed++;
                }
            }

            textWriter.Close();
            // ignore the header row
            return rowsProcessed - 1;

        }

        private void MakeCSVBody(string excludeField)
        {
            //  generate list of field IDs to be excluded
            //  format is ID0000x where x is one based count so convert to zero based
            int excludeFieldID = Int32.Parse(excludeField.Substring(2)) - 1;
            List<string> fieldValuesForExclude = new List<string>();
            foreach(var item in CurrentMappingDefinition.Level2Records)
            {
                fieldValuesForExclude.Add(item[excludeFieldID]);
            }

            CSVDataProcesses = new CSVDataProcesses(CurrentMappingDefinition);

            foreach (var item in CSVDataIn)
            {

                if (CheckRow(item, excludeField, fieldValuesForExclude))
                {
                    var row = MakeCSVRow(item);
                    if (row != null)
                    {
                        CSVDataOut.Add(row);
                    }
                }
            }
        }

        private bool CheckRow(List<string> item, string excludeField, List<string> fieldValuesForExclude)
        {

            if(CurrentMappingDefinition.RequiresLevel2File)
            {
                // if employee number is in level 2 file then ignore -> excludeField
                int checkExcludeFieldID = (CurrentMappingDefinition.MappingFields.FirstOrDefault(x => x.MiddlewareFieldIdentifier == excludeField).SampleID) - 1;
                if (fieldValuesForExclude.Contains(item[checkExcludeFieldID]))
                {
                    return false;
                }


            }

            foreach (var filter in MappingFilters)
            {
                int fieldID = (CurrentMappingDefinition.MappingFields.FirstOrDefault(x => x.SampleField == filter.FieldToMatch).SampleID) - 1;
                switch (filter.FilterMatchType)
                {
                    case FilterMatchType.IsAValidDate:
                        if (CSVDataProcesses.IsDate(item[fieldID]))
                        {
                            return false;
                        }
                        break;
                    case FilterMatchType.StringMatchIgnoreCase:
                        if (item[fieldID].ToLower() == filter.MatchingValue.ToLower())
                        {
                            return false;
                        }
                        break;
                    case FilterMatchType.StringMatchWithCase:
                        if (item[fieldID] == filter.MatchingValue)
                        {
                            return false;
                        }
                        break;
                   
                }
            }
            return true;
        }

        private List<string> MakeCSVRow(List<string> rowIn)
        {
            List<string> rowOut = new List<string>();

            foreach (var item in DefinitionFields)
            {
                string dataItem = MakeCSVDataItem(rowIn, item);
                rowOut.Add(dataItem);
            }

            return rowOut;
        }

        private void MakeHeaderRow()
        {
            List<string> row = new List<string>();

            foreach (var item in DefinitionFields)
            {
                row.Add(item.OutputName);
            }

            CSVDataOut.Add(row);
        }

        private string MakeCSVDataItem(List<string> rownIn, MiddlewareField item)
        {

            MappingField mappingField = CurrentMappingDefinition.MappingFields.DefaultIfEmpty(null).FirstOrDefault(x => x.MiddlewareFieldIdentifier == item.MiddlewareFieldIdentifier);
            if (mappingField == null)
            {
                // field does not exist in mapping
                return CSVDataProcesses.CheckEmptyItem(item);
            }

            return CSVDataProcesses.CheckKnownItem(rownIn[mappingField.SampleID - 1], item);

        }


        private bool LoadCSVPayrollFile(string fileName, bool firstRowHasHeaders)
        {
            CSVDataIn = CSVConvertorFileManagement.LoadCSVFile(fileName, firstRowHasHeaders);

            if (CSVDataIn == null)
            {
                MessageBox.Show("Error reading CSV file");
                return false;
            }

            return true;
        }


        #endregion

       
    }
}
