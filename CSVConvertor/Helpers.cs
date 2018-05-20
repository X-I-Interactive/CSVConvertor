using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CSVConvertor
{
    public enum FileTypeForLoading
    {
        None,
        Level2,
        OptOutChanges,
        OptOutNew,
        Errors
    }

    public class Helpers
    {
        public static DependencyObject GetScrollViewer(DependencyObject dependencyObject)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (dependencyObject is ScrollViewer)
            { return dependencyObject; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var childObject = VisualTreeHelper.GetChild(dependencyObject, i);

                var result = GetScrollViewer(childObject);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }
    }

    public class ListBoxPair
    { 
        public string Key { get; set; }
        public string Value { get; set; }

        public ListBoxPair()
        {
            Key = String.Empty;
            Value = String.Empty;
        }

        public ListBoxPair(string key, string value)
        {
            Key = key;
            Value = value;
        }        
    }

    public class AdditionalFileStatus
    {
        public string FileType { get; set; }
        public int RowCount { get; set; }
        public string DateLastUpdated { get; set; }
    }

    public static class EnumExtractor
    {
        public static Dictionary<dynamic, String> GetValueFromDescription<T>()
        {
            Dictionary<dynamic, String> itemValueDescritpionList = new Dictionary<dynamic, String>();
            dynamic itemValue;
            String itemDescription = String.Empty;

            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();

            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    itemDescription = attribute.Description;
                    itemValue = (dynamic)field.GetValue(null);
                    itemValueDescritpionList.Add(itemValue, itemDescription);
                }

            }
            return itemValueDescritpionList;
        }
    }
}
