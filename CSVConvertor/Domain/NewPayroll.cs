using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Collections;

namespace CSVConvertor.Domain
{
    public class NewPayroll //ITM
    {
        [IsMandatory]
        public String Payroll_Reference { get; set; }
        [IsMandatory]
        public String NI_Number { get; set; }
        [IsMandatory]
        public String Title { get; set; }
        [IsMandatory]
        public String Surname { get; set; }
        [IsMandatory]
        public String Forenames { get; set; }
        [IsMandatory]
        [SpecialProcess("Gender")]
        public String Gender_Text { get; set; }     //  Male/Female
        [IsMandatory]        
        public DateTime Date_of_Birth { get; set; }
        [IsMandatory]
        public DateTime Date_Joined_Company { get; set; }
        public String Address1 { get; set; }
        public String Address2 { get; set; }
        public String Address3 { get; set; }
        public String Address4 { get; set; }
        public String Address5 { get; set; }
        public String Postcode { get; set; }
        public String Country { get; set; }
        public String Email { get; set; }
        [IsMandatory]
        public decimal Earnings { get; set; }
        [IsMandatory]
        [SpecialProcess("YN")]
        public String Holiday_Pay_Included { get; set; }    //  Y/N
        public int No_of_Weeks { get; set; }
        [IsMandatory]
        public String Employer { get; set; }
        [IsMandatory]
        public String Scheme { get; set; }
        public decimal Eee_Cont_Rate { get; set; }
        public decimal Eer_Cont_Rate { get; set; }
        public decimal AVC_Cont_Rate { get; set; }
        [SpecialProcess("ACTIVE")]
        public String Membership_Status { get; set; }   //  ACTIVE/INACTIVE
        [IsMandatory]
        [SpecialProcess("YN")]
        public String Ordinarily_Work_UK { get; set; }  //  Y/N
        [IsMandatory]
        [SpecialProcess("YN")]
        public String Contractual_Membership { get; set; }  //  Y/N
        public DateTime Date_Joined_Scheme { get; set; }
        public DateTime Date_Of_Leaving { get; set; }
        public DateTime Date_Transferred_In_Payroll { get; set; }
        public decimal Employee_Conts { get; set; }
        public decimal Employer_Conts { get; set; }
        public decimal AVC_Conts { get; set; }
        public String Reason_For_Leaving { get; set; }
        public DateTime Date_Left_Company { get; set; }
        public decimal Contribution_Salary { get; set; }
        public DateTime TUPE_Transfer_Date { get; set; }
        public String JobStatus { get; set; }
        public DateTime Date_Transferred_Out_Payroll { get; set; }
        
    }

}
