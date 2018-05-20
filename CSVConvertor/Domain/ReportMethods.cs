using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSVConvertor.Domain
{
    public class ReportMethods
    {
        public static List<String> GetHeadings<T>()
        {
            List<String> headings = new List<string>();

            foreach (PropertyInfo item in (typeof(T)).GetProperties())
            {
                headings.Add(item.Name);
            }

            return headings;
        }

        public static List<String> GetReportLine<T>(T reportLine)
        {
            Object tempItem = null;
            List<String> returnList = new List<string>();
            foreach (PropertyInfo item in (typeof(T)).GetProperties())
            {
                bool isMandatory = ReportMethods.IsPropertyMandatory(item);
                Type type = item.PropertyType;

                tempItem = item.GetValue(reportLine, null) ?? String.Empty;

                if (type == typeof(DateTime))
                {
                    //dd/mm/yyyy
                    DateTime tempDate = (DateTime)tempItem;
                    if (tempDate == DateTime.MinValue)
                    {
                        if (isMandatory)
                        {
                            // flag error
                            returnList.Add("Mandatory");
                        }
                        else
                        {
                            returnList.Add(String.Empty);
                        }
                    }
                    else
                    {
                        returnList.Add(tempDate.ToString("dd/MM/yyyy"));
                    }

                }
                else if (type == typeof(String))
                {

                    if (String.IsNullOrEmpty(tempItem.ToString()))
                    {
                        if (isMandatory)
                        {
                            // flag error
                            returnList.Add("Mandatory");
                        }
                        else
                        { 
                            returnList.Add(String.Empty); 
                        }
                    }
                    else
                    {
                        returnList.Add(tempItem.ToString());
                    }
                }
                else
                {

                    if (String.IsNullOrEmpty(tempItem.ToString()))
                    {
                        if (isMandatory)
                        {
                            // flag error
                            returnList.Add("Mandatory");
                        }
                        else
                        {
                            returnList.Add(String.Empty);
                        }
                    }
                    else
                    { 
                        returnList.Add(tempItem.ToString()); 
                    }
                }
            }

            return returnList;
        }




        private static bool IsPropertyMandatory(PropertyInfo member)
        {
            foreach (object attribute in member.GetCustomAttributes(true))
            {
                if (attribute is IsMandatoryAttribute)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
