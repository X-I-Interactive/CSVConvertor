using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Convertor.Respository.ConvertorClasses;
using Convertor.Respository.DataLayer;
using CsvHelper;

namespace CSVConvertor
{
    /// <summary>
    /// Interaction logic for MappingWindow.xaml
    /// </summary>
    public partial class MappingWindow : Window
    {

        // To do: change arrays to lists.

        public string CSVAppFolderMapping;

        private string SampleFileName = string.Empty;
        private List<string> SampleHeaderRow = new List<string>();
        private List<string> SampleDataRow = new List<string>();
        internal ObservableCollection<ListBoxPair> DefinitionFields = new ObservableCollection<ListBoxPair>();
        ObservableCollection<ListBoxPair> MappingSettingList = new ObservableCollection<ListBoxPair>();

        internal ScrollViewer ScrollViewerMapping = new ScrollViewer();
        internal ScrollViewer ScrollViewerSampleContentList = new ScrollViewer();

        ListBoxPair SelectedMappingItem = new ListBoxPair();
        ListBoxPair SelectedDefinitionItem = new ListBoxPair();

        ObservableCollection<ListBoxPair> DefinitionFiles = new ObservableCollection<ListBoxPair>();
        ObservableCollection<ListBoxPair> MappingFiles = new ObservableCollection<ListBoxPair>();

        //private MappingDefinition CurrentMappingDefinition = new MappingDefinition();
        private List<MappingFilter> MappingFilterList = new List<MappingFilter>();

        bool InScroll = false;

        public MappingWindow(string appFolder)
        {

            CSVAppFolderMapping = appFolder;

            InitializeComponent();

            InitialiseScreenComponents();

        }
        #region "Screen control function"

        private void SampleContentList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewerMapping.ScrollToVerticalOffset(ScrollViewerSampleContentList.VerticalOffset);
        }
        private void CloseOptionWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadDefinitionsFileButton_Click(object sender, RoutedEventArgs e)
        {
            SetupDefinitionList();
        }

        private void LoadMappingButton_Click(object sender, RoutedEventArgs e)
        {
            SetupExistingMapping();
        }

        private void DeleteMappingButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedMapping();
        }

        private void LoadNewSourceFileButton_Click(object sender, RoutedEventArgs e)
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
                SampleFileName = fileDialog.FileName;
                //OriginalFile.Text = fileDialog.FileName;
                LoadDisplaySampleFile(SampleFileName);
            }
        }

        private void SampleContentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!InScroll)
            {
                InScroll = true;
                MappingListBox.SelectedIndex = SampleContentList.SelectedIndex;
                SelectedMappingItem = (ListBoxPair)MappingListBox.SelectedItem;

                ScrollViewerMapping.ScrollToVerticalOffset(ScrollViewerSampleContentList.VerticalOffset);
                InScroll = false;
            }

        }

        private void MappingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!InScroll)
            {
                InScroll = true;
                SampleContentList.SelectedIndex = MappingListBox.SelectedIndex;
                SelectedMappingItem = (ListBoxPair)MappingListBox.SelectedItem;

                ScrollViewerSampleContentList.ScrollToVerticalOffset(ScrollViewerMapping.VerticalOffset);
                InScroll = false;
            }

        }

        private void DefinitionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedDefinitionItem = (ListBoxPair)DefinitionListBox.SelectedItem;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            MoveDefinitionItemToMappingItem();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ReturnMappingItemToDefinitionList();
        }

        private void SaveMappingButton_Click(object sender, RoutedEventArgs e)
        {
            if (MappingNameTextBox.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Please enter a mapping name");
                return;
            }

            SaveCurrentMapping(MappingNameTextBox.Text.Trim());

            MessageBox.Show("Mapping saved");
        }

        private void ImportMappingButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(CSVConvertorFileManagement.ImportMappingFile(CSVAppFolderMapping));
            InitialiseScreenComponents();
        }

        private void buttonSaveFilter_Click(object sender, RoutedEventArgs e)
        {
            CopyFilterBack();
        }

        private void buttonEditFilter_Click(object sender, RoutedEventArgs e)
        {
            CheckEditFilter();
        }

        private void buttonDeleteFilter_Click(object sender, RoutedEventArgs e)
        {
            CheckDeleteFilter();
        }
              


        #endregion

        #region "Helper function"

        private void InitialiseScreenComponents()
        {
            DefinitionFiles = new ObservableCollection<ListBoxPair>();
            DefinitionFiles.Add(new ListBoxPair { Key = "0", Value = "(Select a definition)" });

            foreach (var file in CSVConvertorFileManagement.GetDefinitionFiles(CSVAppFolderMapping, ".xml"))
            {
                DefinitionFiles.Add(new ListBoxPair(file, file));
            }

            DefinitionFileList.DisplayMemberPath = "Value";
            DefinitionFileList.SelectedValuePath = "Key";
            DefinitionFileList.ItemsSource = DefinitionFiles;

            DefinitionFileList.SelectedIndex = 0;

            MappingFiles = new ObservableCollection<ListBoxPair>();
            MappingFiles.Add(new ListBoxPair { Key = "0", Value = "(Select a mapping)" });

            foreach (var file in CSVConvertorFileManagement.GetMappingFiles(CSVAppFolderMapping, ".xml"))
            {
                MappingFiles.Add(new ListBoxPair(file, file));
            }
            MappingFileList.DisplayMemberPath = "Value";
            MappingFileList.SelectedValuePath = "Key";
            MappingFileList.ItemsSource = MappingFiles;

            MappingFileList.SelectedIndex = 0;

            SetFilterSelectors();

        }

        private void SetFilterSelectors()
        {
            // set filter combos            
            var datatTypeList = EnumExtractor.GetValueFromDescription<FilterType>();
            filterTypeSelector.ItemsSource = datatTypeList;
            filterTypeSelector.DisplayMemberPath = "Value";
            filterTypeSelector.SelectedValuePath = "Key";

            //matchTypeSelector
            var datatMatchList = EnumExtractor.GetValueFromDescription<FilterMatchType>();
            matchTypeSelector.ItemsSource = datatMatchList;
            matchTypeSelector.DisplayMemberPath = "Value";
            matchTypeSelector.SelectedValuePath = "Key";

        }

        private void CopyFilterBack()
        {
            MappingFilter mappingFilter = new MappingFilter();

            mappingFilter.FilterName = textBoxFilterName.Text.Trim();
            mappingFilter.FilterType = (FilterType)(filterTypeSelector.SelectedValue ?? FilterType.Exclude);
            mappingFilter.FieldToMatch = filterFieldNames.Text;
            mappingFilter.FilterMatchType = (FilterMatchType)(matchTypeSelector.SelectedValue ?? FilterMatchType.StringMatchIgnoreCase);
            mappingFilter.MatchingValue = textBoxTextToMatch.Text.Trim();

            if (ValidateFilter(mappingFilter))
            {
                AddUpdateFilter(mappingFilter);
                ClearFilterSettings();
            }

        }

        private void AddUpdateFilter(MappingFilter mappingFilter)
        {
            if (MappingFilterList.FirstOrDefault(x => x.FilterName == mappingFilter.FilterName) == null)
            {
                // new filter
                MappingFilterList.Add(mappingFilter);
            }
            else
            {
                // replace existing
                MappingFilterList[MappingFilterList.FindIndex(x => x.FilterName == mappingFilter.FilterName)] = mappingFilter;
            }

            DisplayCurrentFilterList();
        }

        private void CheckEditFilter()
        {
            if (currentFiltersListbox.SelectedIndex >=0)
            {
                //System.Diagnostics.Debug.Write(currentFiltersListbox.SelectedValue.ToString());
                var editItem = MappingFilterList.Find(x => x.FilterName == currentFiltersListbox.SelectedValue.ToString());
                textBoxFilterName.Text = editItem.FilterName;
                filterTypeSelector.SelectedValue = editItem.FilterType;
                filterFieldNames.Text = editItem.FieldToMatch;
                matchTypeSelector.SelectedValue = editItem.FilterMatchType;
                textBoxTextToMatch.Text = editItem.MatchingValue;
            }
        }

        private void CheckDeleteFilter()
        {
            if (currentFiltersListbox.SelectedIndex >= 0)
            {
                if (MessageBox.Show("Are you sure you want to delete this filter", "Delete filter", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    MappingFilterList.RemoveAt(MappingFilterList.FindIndex(x => x.FilterName == currentFiltersListbox.SelectedValue.ToString()));
                    DisplayCurrentFilterList();
                }
                
            }
        }

        private void DisplayCurrentFilterList()
        {
            List<ListBoxPair> itemSource = new List<ListBoxPair>();

            foreach (var item in MappingFilterList)
            {
                string tempItem = String.Format("{0}: {1} for field '{2}'; match is {3}",
                            item.FilterName, item.FilterType.ToString(), item.FieldToMatch, item.FilterMatchType);
                if (item.MatchingValue != string.Empty)
                {
                    tempItem += " against '" + item.MatchingValue + "'";
                }

                ListBoxPair listItem = new ListBoxPair(item.FilterName, tempItem);
                itemSource.Add(listItem);
            }

            currentFiltersListbox.ItemsSource = itemSource;
            currentFiltersListbox.DisplayMemberPath = "Value";
            currentFiltersListbox.SelectedValuePath = "Key";            
        }

        private bool ValidateFilter(MappingFilter mappingFilter)
        {
            if (mappingFilter.FilterName == string.Empty)
            {
                MessageBox.Show("Please add a name for the filter");
                return false;
            }
            if (mappingFilter.FieldToMatch == string.Empty)
            {
                MessageBox.Show("Please add a field to match");
                return false;
            }

            if (mappingFilter.FieldToMatch == "-")
            {
                MessageBox.Show("Please add a named field to match");
                return false;
            }

            return true;
        }

        private void SetupDefinitionList()
        {
            if (DefinitionFileList.SelectedIndex < 1)
            {
                // please select
                DefinitionListBox.ItemsSource = null;
                return;
            }

            LoadMiddlewareDefinition(((ListBoxPair)DefinitionFileList.SelectedItem).Value);
            SetDefinitionListBox();

        }

        private void SetDefinitionListBox()
        {
            DefinitionListBox.ItemsSource = DefinitionFields;
            DefinitionListBox.DisplayMemberPath = "Value";
            DefinitionListBox.SelectedValuePath = "Key";
        }

        private void LoadMiddlewareDefinition(string definitionFile)
        {
            List<MiddlewareField> middlewareFields = CSVConvertorFileManagement.ReadDefinitionFile(definitionFile, CSVAppFolderMapping, ".xml");
            DefinitionFields = new ObservableCollection<ListBoxPair>(middlewareFields.Select(x => new ListBoxPair { Key = x.MiddlewareFieldIdentifier, Value = x.Description }));
        }

        private void DeleteSelectedMapping()
        {
            //  load from file
            if (MappingFileList.SelectedIndex < 1)
            {
                MessageBox.Show("No mapping selected");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this mapping", "Delete mapping file", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (CSVConvertorFileManagement.DeleteMappingFile(((ListBoxPair)MappingFileList.SelectedItem).Value, CSVAppFolderMapping, ".xml"))
                {
                    MessageBox.Show("Mapping deleted");
                    InitialiseScreenComponents();

                }
                else
                {
                    MessageBox.Show("Error deleting mapping");
                }
            }
        }

        private void SetupExistingMapping()
        {
            //  load from file
            if (MappingFileList.SelectedIndex < 1)
            {
                MessageBox.Show("No mapping selected");
                return;
            }
            MappingDefinition mappingDefinition = CSVConvertorFileManagement.ReadMappingFile(((ListBoxPair)MappingFileList.SelectedItem).Value, CSVAppFolderMapping, ".xml");

            // file/combo settings
            MappingNameTextBox.Text = ((ListBoxPair)MappingFileList.SelectedItem).Value;
            DefinitionFileList.SelectedValue = mappingDefinition.DefinitionFile;

            LoadMiddlewareDefinition(mappingDefinition.DefinitionFile);
            MappingSettingList = new ObservableCollection<ListBoxPair>();
            ListBoxPair listBoxPair = new ListBoxPair();
            int mappingFieldID = 0;
            string middlewareFieldIdentifier = string.Empty;

            SampleHeaderRow = new List<string>();
            SampleDataRow = new List<string>();

            foreach (MappingField mappingField in mappingDefinition.MappingFields)
            {
                int idx = mappingField.SampleID;
                //  set sample value and restore Sample and Header arrays
                SampleHeaderRow.Add(mappingField.SampleField);
                SampleDataRow.Add(mappingField.SampleValue);


                middlewareFieldIdentifier = mappingField.MiddlewareFieldIdentifier;

                listBoxPair = new ListBoxPair();
                if (Int32.TryParse(mappingField.MiddlewareFieldIdentifier, out mappingFieldID))
                {
                    //  convention is that unset mapping has negative integer ID
                    // if mapping set then delete from DefintionFields and Add to MappingFields
                    listBoxPair = new ListBoxPair(((idx + 1) * -1).ToString(), "-");
                }
                else
                {
                    // or set actual mapping and remove from definition list
                    listBoxPair = DefinitionFields.FirstOrDefault(x => x.Key == middlewareFieldIdentifier);
                    DefinitionFields.Remove(listBoxPair);

                }
                MappingSettingList.Add(listBoxPair);
            }


            SetDefinitionListBox();
            SetMappingList();
            ClearFilterSettings();
            DisplaySampleFile();
            SetFilterFieldNames();

            CompanyNameTextBox.Text = mappingDefinition.CompanyName;
            MappingFilterList = mappingDefinition.MappingFilters;

            checkBoxLevel2.IsChecked = mappingDefinition.RequiresLevel2File;
            checkBoxOptOut.IsChecked = mappingDefinition.RequiresOptOutFile;
            checkBoxErrorFile.IsChecked = mappingDefinition.CanHaveErrorFile;

            DisplayCurrentFilterList();

            SetScrollViewer();

        }

        private void LoadDisplaySampleFile(string fileName)
        {
            // https://github.com/JoshClose/CsvHelper

            if (LoadSampleFile(fileName))
            {
                DisplaySampleFile();
                DisplayEmptyMappingList();
                SetScrollViewer();
            }

        }

        private void SetScrollViewer()
        {
            ScrollViewerMapping = (ScrollViewer)Helpers.GetScrollViewer(MappingListBox);
            ScrollViewerSampleContentList = (ScrollViewer)Helpers.GetScrollViewer(SampleContentList);
        }

        private void DisplayEmptyMappingList()
        {
            MappingSettingList = new ObservableCollection<ListBoxPair>();
            for (int idx = 0; idx < SampleHeaderRow.Count(); idx++)
            {
                ListBoxPair item = new ListBoxPair(((idx + 1) * -1).ToString(), "-");
                MappingSettingList.Add(item);
            }

            SetMappingList();

        }

        private void SetMappingList()
        {
            MappingListBox.ItemsSource = MappingSettingList;
            MappingListBox.DisplayMemberPath = "Value";
            MappingListBox.SelectedValuePath = "Key";

        }

        private void SetFilterFieldNames()
        {
            List<ListBoxPair> itemSource = new List<ListBoxPair>();

            foreach(string fieldName in SampleHeaderRow)
            {
                ListBoxPair item = new ListBoxPair(fieldName, fieldName);
                itemSource.Add(item);
            }

            filterFieldNames.ItemsSource = itemSource;
            filterFieldNames.DisplayMemberPath = "Value";
            filterFieldNames.SelectedValuePath = "Key";
        }

        private void ClearFilterSettings()
        {
            textBoxFilterName.Text = string.Empty;
            filterTypeSelector.SelectedIndex = 0;
            filterFieldNames.SelectedIndex = -1;
            matchTypeSelector.SelectedIndex = 0;
            textBoxTextToMatch.Text = string.Empty;

        }

        private void DisplaySampleFile()
        {
            List<ListBoxPair> itemSource = new List<ListBoxPair>();

            for (int idx = 0; idx < SampleHeaderRow.Count(); idx++)
            {
                ListBoxPair item = new ListBoxPair((idx + 1).ToString(), string.Format("{0} ({1})", SampleHeaderRow[idx], SampleDataRow[idx]));
                itemSource.Add(item);
            }

            SampleContentList.ItemsSource = itemSource;
            SampleContentList.DisplayMemberPath = "Value";
            SampleContentList.SelectedValuePath = "Key";

        }

        private bool LoadSampleFile(string fileName)
        {
            StreamReader textReader = new StreamReader(fileName);
            CsvParser parser = new CsvParser(textReader);
            string[] sampleHeaderRow = null;
            string[] sampleDataRow = null;
            try
            {
                while (true)
                {
                    sampleHeaderRow = parser.Read();
                    sampleDataRow = parser.Read();
                    break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in sample file");
                return false;
            }


            SampleHeaderRow = sampleHeaderRow.ToList();
            SampleDataRow = sampleDataRow.ToList();

            parser.Dispose();
            textReader.Close();

            return true;
        }

        private void MoveDefinitionItemToMappingItem()
        {

            if (MappingListBox.SelectedIndex != -1 && DefinitionListBox.SelectedIndex != -1)
            {
                // only move into mapping area if there are items selected from both lists and the mapping list item is not set
                if (Int32.Parse(SelectedMappingItem.Key) < 0)
                {
                    MappingSettingList[MappingListBox.SelectedIndex] = SelectedDefinitionItem;
                    MappingListBox.Items.Refresh();                    

                    DefinitionFields.Remove(SelectedDefinitionItem);
                    DefinitionListBox.Items.Refresh();
                }
            }
        }

        private void ReturnMappingItemToDefinitionList()
        {
            // definiton item keys start with an identifier string, dummys are -ve numbers
            int tempVal = 0;
            bool isDummyItem = int.TryParse(SelectedMappingItem.Key, out tempVal);
            if (MappingListBox.SelectedIndex != -1 && !isDummyItem)
            {
                // mapping item selected and it is a definition item
                // create dummy mapping item
                ListBoxPair item = (ListBoxPair)SampleContentList.SelectedItem;
                item.Value = "-";
                item.Key = (Convert.ToInt32(item.Key) * -1).ToString();
                DefinitionFields.Add(SelectedMappingItem);
                MappingSettingList[MappingListBox.SelectedIndex] = item;

                MappingListBox.Items.Refresh();
                DefinitionListBox.Items.Refresh();                
            }
        }

        private void SaveCurrentMapping(string mappingFileName)
        {
            // TO DO - set drop down/mapping file values

            MappingDefinition currentMappingDefinition = new MappingDefinition();
            currentMappingDefinition.DefinitionFile = ((ListBoxPair)DefinitionFileList.SelectedItem).Value;
            currentMappingDefinition.CompanyName = CompanyNameTextBox.Text.Trim();
            currentMappingDefinition.MappingFilters = MappingFilterList;
            currentMappingDefinition.RequiresLevel2File = checkBoxLevel2.IsChecked ?? false;
            currentMappingDefinition.RequiresOptOutFile = checkBoxOptOut.IsChecked ?? false;
            currentMappingDefinition.CanHaveErrorFile = checkBoxErrorFile.IsChecked ?? false;


            MappingField mappingField = new MappingField();

            for (int idx = 0; idx < SampleHeaderRow.Count(); idx++)
            {
                mappingField = new MappingField();
                mappingField.SampleID = idx + 1;
                mappingField.SampleField = SampleHeaderRow[idx];
                mappingField.SampleValue = SampleDataRow[idx];
                var item = MappingSettingList[idx];
                mappingField.MiddlewareFieldIdentifier = item.Key;

                currentMappingDefinition.MappingFields.Add(mappingField);
            }

            CSVConvertorFileManagement.SaveMappingFile(currentMappingDefinition, mappingFileName, CSVAppFolderMapping, ".xml");

        }


        #endregion
        
    }
}
