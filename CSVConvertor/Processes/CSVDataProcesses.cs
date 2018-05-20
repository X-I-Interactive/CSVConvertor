using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Convertor.Respository.ConvertorClasses;

namespace CSVConvertor.Processes
{
    public class CSVDataProcesses
    {
        private Dictionary<string, string> YesNoLookup = new Dictionary<string, string>() { { "Y", "Y" }, { "N", "N" } };
        private Dictionary<string, string> GenderLookup = new Dictionary<string, string>() { { "M", "Male" }, { "F", "Female" } };

        public MappingDefinition MappingDefinition { get; set; }

        public CSVDataProcesses()
        {
            
        }

        public CSVDataProcesses(MappingDefinition mappingDefinition)
        {
            MappingDefinition = mappingDefinition;
        }

        public string CheckEmptyItem(MiddlewareField fieldItem)
        {
            return SetDefaultValue(fieldItem);
        }

        public bool IsDate(string itemToCheck)
        {
            DateTime tempDate;
            return DateTime.TryParse(itemToCheck, out tempDate);
        }

        public string CheckKnownItem(string dataItem, MiddlewareField fieldItem)
        {
            switch (fieldItem.SpecialType)
            {
                case MiddlewareSpecialType.RelatedValue:
                case MiddlewareSpecialType.None:
                    return CheckDataType(dataItem, fieldItem);
                case MiddlewareSpecialType.YesNo:
                    return CheckYesNo(dataItem, fieldItem.IsMandatory);
                case MiddlewareSpecialType.Gender:
                    return CheckGender(dataItem, fieldItem.IsMandatory);
                case MiddlewareSpecialType.ActiveInactive:
                    return String.Empty;

            }
            return dataItem;
        }

        private string CheckDataType(string dataItem, MiddlewareField fieldItem)
        {
            DateTime tempDate;
            float tempFloat;
            int tempInt;

            switch (fieldItem.MiddlewareDataType)
            {
                case MiddlewareDataType.StringType:
                    if (string.IsNullOrWhiteSpace(dataItem))
                    {
                        if (!string.IsNullOrWhiteSpace(fieldItem.DefaultValue))
                        {
                            return SetDefaultValue(fieldItem);
                        }
                        break;
                    }
                    return dataItem;
                case MiddlewareDataType.DecimalDataType:
                case MiddlewareDataType.CurrencyType:
                    if (float.TryParse(dataItem, out tempFloat))
                    {
                        return dataItem;
                    }
                    break;
                case MiddlewareDataType.IntegerType:
                    if (Int32.TryParse(dataItem, out tempInt))
                    {
                        return dataItem;
                    }
                    break;
                case MiddlewareDataType.DateType:
                    if (DateTime.TryParse(dataItem, out tempDate))
                    {
                        return tempDate.ToString("dd/MM/yyyy");
                    }
                    break;
            }

            return (fieldItem.IsMandatory) ? "error" : string.Empty;
        }

        private string SetDefaultValue(MiddlewareField fieldItem)
        {
            if (fieldItem.FixedValue == MiddlewareFixedValue.CompanyName)
            {
                return MappingDefinition.CompanyName;
            }
            return fieldItem.DefaultValue;
        }

        private string CheckYesNo(string dataItem, bool isMandatory)
        {
            // Y/N
            if (string.IsNullOrWhiteSpace(dataItem))
            {
                return (isMandatory) ? "error" : string.Empty;
            }
            string tempValue = dataItem.Substring(0, 1).ToUpper();
            if (YesNoLookup.ContainsKey(tempValue))
            {
                return YesNoLookup[tempValue];
            }
            return (isMandatory) ? "error" : string.Empty;
        }

        private string CheckGender(string dataItem, bool isMandatory)
        {
            //  Male/Female
            if (string.IsNullOrWhiteSpace(dataItem))
            {
                return (isMandatory) ? "error" : string.Empty;
            }
            string tempValue = dataItem.Substring(0, 1).ToUpper();

            if (GenderLookup.ContainsKey(tempValue))
            {
                return GenderLookup[tempValue];
            }

            return (isMandatory) ? "error" : string.Empty;

        }
    }
}
