using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Convertor.Respository.ConvertorClasses
{
    public class MappingFilter
    {
        public string FilterName { get; set; }
        public FilterType FilterType { get; set; }
        public string FieldToMatch { get; set; }
        public FilterMatchType FilterMatchType { get; set; }
        public string MatchingValue { get; set; }


        public MappingFilter()
        {
            FilterType = FilterType.Exclude;;
        }
    }

    public enum FilterType
    {
        [Description("Exclude")]
        Exclude = 1,

        [Description("Filter by imported list")]
        FilterList = 2
    }

    public enum FilterMatchType
    {
        [Description("Equals, ignore case")]
        StringMatchIgnoreCase = 1,
        [Description("Equals, including case")]
        StringMatchWithCase = 2,
        [Description("Is a valid date field")]
        IsAValidDate = 3
    }
}
