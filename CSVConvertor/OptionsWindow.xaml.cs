using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Convertor.Respository.ConvertorClasses;
using Convertor.Respository.DataLayer;

namespace CSVConvertor
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// Detect any XML definition files and list for opening, if none then assume (new)
    /// If file select then enable load button
    /// Load/dislay field list, actions available: edit, delete, move up, move down, add new
    /// </summary>
    public partial class OptionsWindow : Window
    {

        public string CSVAppFolderOptions = string.Empty;

        internal ObservableCollection<MiddlewareField> DefinitionFields = new ObservableCollection<MiddlewareField>();

        internal MiddlewareField CurrentFieldDefinition = new MiddlewareField();

        public OptionsWindow(string appFolder)
        {

            CSVAppFolderOptions = appFolder;

            InitializeComponent();

            InitialiseScreenComponents();

        }

        private void CloseOptionWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region "Control functions"
        private void DefinitionFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //MessageBox.Show(DefinitionFileList.SelectedIndex.ToString());
            EditCreateFieldsButton.IsEnabled = (DefinitionFileList.SelectedIndex != -1);
        }

        private void EditCreateFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            SetupDefinitionList();

        }

        private void NewField_Click(object sender, RoutedEventArgs e)
        {
            FieldItemsPanel.IsEnabled = true;
            EditFieldCloseButtonsPanel.IsEnabled = true;
            CurrentFieldDefinition = new MiddlewareField();

            DisplayDefinitionField(CurrentFieldDefinition);
        }

        private void EditField_Click(object sender, RoutedEventArgs e)
        {
            SetFieldToEdit();
        }

        private void definitionListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetEditButtonsToOn(definitionListBox.SelectedIndex);
            SetFieldToEdit();
        }

        private void DeleteField_Click(object sender, RoutedEventArgs e)
        {
            RemoveFieldDefinition(CurrentFieldDefinition);
        }

        private void FieldUp_Click(object sender, RoutedEventArgs e)
        {
            MoveFieldInList(-1);
        }

        private void MoveFieldInList(int changeDirection)
        {
            int currentID = definitionListBox.SelectedIndex;
            int otherID = currentID + changeDirection;
            swapDefinitions(currentID, otherID);
            definitionListBox.SelectedIndex = otherID;
        }

        private void FieldDown_Click(object sender, RoutedEventArgs e)
        {
            //int currentID = definitionListBox.SelectedIndex;
            //int otherID = currentID + 1;
            //swapDefinitions(currentID, otherID);
            MoveFieldInList(1);
        }

        private void CancelFieldEditButton_Click(object sender, RoutedEventArgs e)
        {
            // clear/reset all fields and lock controls
            ClearFieldItemsPanel();
        }

        private void SaveFieldEditButton_Click(object sender, RoutedEventArgs e)
        {
            //  copy back
            //  validate
            // add to/replace in list and display list
            CopybackFieldDefinition();

            if (!ValidateFieldDefinition())
            {
                return;
            }
            SaveFieldDefinitionToList();
            ClearFieldItemsPanel();

        }

        private void SaveFieldListButton_Click(object sender, RoutedEventArgs e)
        {
            //  is there a file name
            //  is there more than one field
            //  YES - save
            if (DefinitionFileNameTextBox.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Please add a name to save the definitions under");
                return;
            }

            if (DefinitionFields.Count == 0)
            {
                MessageBox.Show("There are no definitions to save");
                return;
            }

            SaveCurrentDefinitionList(DefinitionFileNameTextBox.Text.Trim());

            InitialiseScreenComponents();
        }

        private void DeleteFieldListButton_Click(object sender, RoutedEventArgs e)
        {
            if (DefinitionFileNameTextBox.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Please set the name of the definition set to delete");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this definition", "Delete definition file", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (CSVConvertorFileManagement.DeleteDefinitionFile(DefinitionFileNameTextBox.Text.Trim(), CSVAppFolderOptions, ".xml"))
                {
                    MessageBox.Show("Definition deleted");
                    InitialiseScreenComponents();
                }
                else
                {
                    MessageBox.Show("Error deleting definition file");
                }                    
            }
        }

        private void definitionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                //  nothing selected - reset
                SetEditButtonsToDefaultState();

            }
            else
            {
                SetEditButtonsToOn(definitionListBox.SelectedIndex);
            }
        }

        private void ImportFieldListButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(CSVConvertorFileManagement.ImportDefinitionFile(CSVAppFolderOptions));

            InitialiseScreenComponents();
        }

        #endregion
        #region "Helper function"

        private void InitialiseScreenComponents()
        {
            //  definition files combo
            // load existing file list

            List<ListBoxPair> definitionFilePairs = new List<ListBoxPair>();
            definitionFilePairs.Add(new ListBoxPair("0", "(new)"));

            foreach (var file in CSVConvertorFileManagement.GetDefinitionFiles(CSVAppFolderOptions, ".xml"))
            {
                definitionFilePairs.Add(new ListBoxPair(file, file));
            }

            DefinitionFileList.DisplayMemberPath = "Value";
            DefinitionFileList.SelectedValuePath = "Key";

            DefinitionFileList.ItemsSource = definitionFilePairs;

            //  field type combo
            var datatTypeList = EnumExtractor.GetValueFromDescription<MiddlewareDataType>();
            FieldTypeSelector.ItemsSource = datatTypeList;
            FieldTypeSelector.DisplayMemberPath = "Value";
            FieldTypeSelector.SelectedValuePath = "Key";

            //  special types combo
            var specialTypeList = EnumExtractor.GetValueFromDescription<MiddlewareSpecialType>();
            SpecialTypeSelector.ItemsSource = specialTypeList;
            SpecialTypeSelector.DisplayMemberPath = "Value";
            SpecialTypeSelector.SelectedValuePath = "Key";

            var fixedValueList = EnumExtractor.GetValueFromDescription<MiddlewareFixedValue>();
            FixedValueSelector.ItemsSource = fixedValueList;
            FixedValueSelector.DisplayMemberPath = "Value";
            FixedValueSelector.SelectedValuePath = "Key";

            //  panels            
            FieldListButtonsPanel.IsEnabled = false;
            FieldItemsPanel.IsEnabled = false;

            //  buttons
            EditCreateFieldsButton.IsEnabled = false;

            //  field items
            ClearFieldItemsPanel();

            definitionListBox.ItemsSource = null;
            DefinitionFileNameTextBox.Text = string.Empty;

            // variables
            DefinitionFields = new ObservableCollection<MiddlewareField>();

        }

        private void SetupDefinitionList()
        {
            if (DefinitionFileList.SelectedIndex > 0)
            {
                DefinitionFileNameTextBox.Text = ((ListBoxPair)DefinitionFileList.SelectedItem).Value;
                //  load from file
                DefinitionFields = new ObservableCollection<MiddlewareField>(CSVConvertorFileManagement.ReadDefinitionFile(DefinitionFileNameTextBox.Text, CSVAppFolderOptions, ".xml"));
            }
            else
            {
                DefinitionFileNameTextBox.Text = string.Empty;
                DefinitionFields = new ObservableCollection<MiddlewareField>();

            }

            definitionListBox.ItemsSource = DefinitionFields;
            definitionListBox.DisplayMemberPath = "Description";
            definitionListBox.SelectedValuePath = "MiddlewareFieldID";


            //  if clicked then either new or already loaded
            //  so enable edit panels
            FieldListPanel.IsEnabled = true;
            FieldListButtonsPanel.IsEnabled = true;
            SetEditButtonsToDefaultState();
        }

        private void SaveCurrentDefinitionList(string definitionFileName)
        {
            for (int idx = 0; idx < DefinitionFields.Count(); idx++)
            {
                // reset the IDs
                DefinitionFields[idx].MiddlewareFieldID = idx + 1;
                if (DefinitionFields[idx].MiddlewareFieldIdentifier == string.Empty)
                {
                    DefinitionFields[idx].MiddlewareFieldIdentifier = string.Format("ID{0:0000}", DefinitionFields[idx].MiddlewareFieldID);
                }
            }
            CSVConvertorFileManagement.SaveDefinitionFile(DefinitionFields.ToList(), definitionFileName, CSVAppFolderOptions, ".xml");
        }

        private void SetEditButtonsToDefaultState()
        {
            //  disable all buttons except new
            EditField.IsEnabled = false;
            NewField.IsEnabled = true;
            DeleteField.IsEnabled = false;
            FieldUp.IsEnabled = false;
            FieldDown.IsEnabled = false;
        }

        private void SetEditButtonsToOn(int listIndex)
        {
            EditField.IsEnabled = true;
            NewField.IsEnabled = true;
            DeleteField.IsEnabled = true;
            FieldUp.IsEnabled = (listIndex > 0);
            FieldDown.IsEnabled = (listIndex < DefinitionFields.Count() - 1);

            CurrentFieldDefinition = DefinitionFields[listIndex];
        }

        private void SetFieldToEdit()
        {
            FieldItemsPanel.IsEnabled = true;
            EditFieldCloseButtonsPanel.IsEnabled = true;

            DisplayDefinitionField(CurrentFieldDefinition);
        }

        private void ClearFieldItemsPanel()
        {
            EditFieldCloseButtonsPanel.IsEnabled = false;
            FieldItemsPanel.IsEnabled = false;

            FieldTypeSelector.SelectedValue = MiddlewareDataType.NoType;
            SpecialTypeSelector.SelectedValue = MiddlewareSpecialType.None;
            FixedValueSelector.SelectedValue = MiddlewareFixedValue.None;

            DescriptionTextBox.Text = string.Empty;
            FieldnameTextBox.Text = string.Empty;
            DefaltValueTextBox.Text = string.Empty;

            IsMandatoryField.IsChecked = false;

        }

        private bool ValidateFieldDefinition()
        {

            StringBuilder errorMessages = new StringBuilder();

            if (CurrentFieldDefinition.MiddlewareDataType == MiddlewareDataType.NoType)
            {
                errorMessages.AppendLine("Please select a data type");

            }
            if (string.IsNullOrEmpty(CurrentFieldDefinition.Description))
            {
                errorMessages.AppendLine("Please select a description");
            }
            if (string.IsNullOrEmpty(CurrentFieldDefinition.OutputName))
            {
                errorMessages.AppendLine("Please select a field name");
            }

            if (errorMessages.Length > 0)
            {
                errorMessages.AppendLine("Field definition was not copied back");
                MessageBox.Show(errorMessages.ToString());
                return false;
            }

            return true;
        }

        private void DisplayDefinitionField(MiddlewareField currentFieldDefinition)
        {
            FieldTypeSelector.SelectedValue = currentFieldDefinition.MiddlewareDataType;
            SpecialTypeSelector.SelectedValue = currentFieldDefinition.SpecialType;
            FixedValueSelector.SelectedValue = currentFieldDefinition.FixedValue;

            DescriptionTextBox.Text = currentFieldDefinition.Description;
            FieldnameTextBox.Text = currentFieldDefinition.OutputName;
            DefaltValueTextBox.Text = currentFieldDefinition.DefaultValue;

            IsMandatoryField.IsChecked = currentFieldDefinition.IsMandatory;

        }


        private void CopybackFieldDefinition()
        {
            CurrentFieldDefinition.MiddlewareDataType = (MiddlewareDataType)FieldTypeSelector.SelectedValue;
            CurrentFieldDefinition.SpecialType = (MiddlewareSpecialType)SpecialTypeSelector.SelectedValue;
            CurrentFieldDefinition.FixedValue = (MiddlewareFixedValue)FixedValueSelector.SelectedValue;


            CurrentFieldDefinition.Description = DescriptionTextBox.Text.Trim();
            CurrentFieldDefinition.OutputName = FieldnameTextBox.Text.Trim();
            CurrentFieldDefinition.DefaultValue = DefaltValueTextBox.Text.Trim();

            CurrentFieldDefinition.IsMandatory = IsMandatoryField.IsChecked ?? false;

        }

        private void SaveFieldDefinitionToList()
        {
            if (CurrentFieldDefinition.MiddlewareFieldID == 0)
            {
                //  it's new to create ID, add to Dictionary, add to list
                if (DefinitionFields.Count() == 0)
                {
                    CurrentFieldDefinition.MiddlewareFieldID = 1;
                }
                else
                {
                    CurrentFieldDefinition.MiddlewareFieldID = DefinitionFields.Max(x => x.MiddlewareFieldID) + 1;
                }

                //  create a premanent ID, not sequence specific
                CurrentFieldDefinition.MiddlewareFieldIdentifier = string.Format("ID{0:0000}", CurrentFieldDefinition.MiddlewareFieldID);
                DefinitionFields.Add(CurrentFieldDefinition);                
            }
            else
            {
                //  replace in collection
                DefinitionFields[DefinitionFields.IndexOf(DefinitionFields.First(x => x.MiddlewareFieldID == CurrentFieldDefinition.MiddlewareFieldID))] = CurrentFieldDefinition;
                definitionListBox.Items.Refresh();

            }
            definitionListBox.ScrollIntoView(CurrentFieldDefinition);
        }

        private void swapDefinitions(int currentID, int otherID)
        {
            MiddlewareField tempDefinition = DefinitionFields[currentID];

            DefinitionFields[currentID] = DefinitionFields[otherID];
            DefinitionFields[otherID] = tempDefinition;

            definitionListBox.Items.Refresh();

        }

        private void RemoveFieldDefinition(MiddlewareField currentFieldDefinition)
        {
            DefinitionFields.Remove(currentFieldDefinition);
            definitionListBox.Items.Refresh();
        }

        #endregion

        
    }
}
