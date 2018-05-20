using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSVConvertor.Domain
{
    public class OriginalPayroll
    {
        public String Employee_Number { get; set; }
        public String Title { get; set; }
        public String Forename { get; set; }
        public String Surname { get; set; }
        public String System_Status { get; set; }   // map to ??
        public String NI_Number { get; set; }
        public DateTime Date_of_Birth { get; set; }
        public String Sex { get; set; }
        public String Address_Line_1 { get; set; }
        public String Address_Line_2 { get; set; }
        public String Address_Line_3 { get; set; }
        public String Address_Line_4 { get; set; }
        public String Postcode { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime Leaving_Date { get; set; }
        public String Job_Title { get; set; }      // map to ??
        public String Work_Email_Address { get; set; }
        public String Payroll { get; set; }     // map to ??
        public String Payroll_Type { get; set; }     // map to ??
        public Decimal Period_Earnings { get; set; }
        public String Works_in_the_UK { get; set; }
        public int Period_Length { get; set; }     // map to ??
        public String Protected_Status { get; set; }     // map to ??

        public OriginalPayroll()
        {
            Date_of_Birth = DateTime.MinValue;
            Start_Date = DateTime.MinValue;
            Leaving_Date = DateTime.MinValue;
        }
    }

    public sealed class OriginalPayrollMap : CsvHelper.Configuration.CsvClassMap<OriginalPayroll>
    {
        public OriginalPayrollMap()
        {
            Map(m => m.Employee_Number).Name("Employee Number");
            Map(m => m.Title).Name("Title");
            Map(m => m.Forename).Name("Forename");
            Map(m => m.Surname).Name("Surname");
            Map(m => m.System_Status).Name("System Status");
            Map(m => m.NI_Number).Name("NI Number");
            Map(m => m.Date_of_Birth).Name("Date of Birth");
            Map(m => m.Sex).Name("Sex");            
            Map(m => m.Address_Line_1).Name("Address Line 1");
            Map(m => m.Address_Line_2).Name("Address Line 2");
            Map(m => m.Address_Line_3).Name("Address Line 3");
            Map(m => m.Address_Line_4).Name("Address Line 4");
            Map(m => m.Postcode).Name("Postcode");
            Map(m => m.Start_Date).Name("Start Date");
            Map(m => m.Leaving_Date).Name("Leaving Date");
            Map(m => m.Job_Title).Name("Job Title");
            Map(m => m.Work_Email_Address).Name("Work Email Address");
            Map(m => m.Payroll).Name("Payroll");
            Map(m => m.Payroll_Type).Name("Payroll Type");
            Map(m => m.Period_Earnings).Name("Period Earnings");
            Map(m => m.Works_in_the_UK).Name("Works in the UK");
            Map(m => m.Period_Length).Name("Period Length");            
            Map(m => m.Protected_Status).Name("Protected Status");            
        }
    }
}
