using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convertor.Respository.ConvertorClasses
{
    public class MiddlewareField
    {
        public int MiddlewareFieldID { get; set; }
        public string MiddlewareFieldIdentifier { get; set; }
        public bool IsMandatory { get; set; }
        public MiddlewareDataType MiddlewareDataType { get; set; }
        public string Description { get; set; }
        public string OutputName { get; set; }
        public string DefaultValue { get; set; }
        public MiddlewareFixedValue FixedValue { get; set; }
        public MiddlewareSpecialType SpecialType { get; set; }

        public MiddlewareField()
        {
            MiddlewareFieldID = 0;
            IsMandatory = false;
            MiddlewareDataType = MiddlewareDataType.StringType;
            SpecialType = MiddlewareSpecialType.None;
            MiddlewareFieldIdentifier = string.Empty;
        }


    }

    public enum MiddlewareDataType
    {
        [Description("No data type set")]
        NoType = 0,
        [Description("Integer data type")]
        IntegerType = 1,
        [Description("Decimal data type")]
        DecimalDataType = 5,
        [Description("Currency data type")]
        CurrencyType = 2,
        [Description("Text data type")]
        StringType = 3,
        [Description("Date data type")]
        DateType = 4
    }

    public enum MiddlewareSpecialType
    {
        [Description("No special type")]
        None,
        [Description("Yes/no type")]
        YesNo,
        [Description("Male/female type")]
        Gender,
        [Description("Active/inactive type")]
        ActiveInactive,
        [Description("Value from other source")]
        RelatedValue

    }

    public enum MiddlewareFixedValue
    {
        [Description("No fixed value")]
        None,
        [Description("Company name")]
        CompanyName       
    }
}


