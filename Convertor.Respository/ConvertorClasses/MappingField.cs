using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Convertor.Respository.ConvertorClasses
{
    public enum FileErrorAction
    {
        NoAction,
        DeleteAll,
        DeleteFirst,
        DeleteLast
    }
    public class MappingField
    {
        public string SampleField { get; set; }
        public string SampleValue { get; set; }
        public int SampleID { get; set; }
        public string MiddlewareFieldIdentifier { get; set; }

        public MappingField()
        {
            SampleID = 0;
        }
    }

    public class OptOutRecord
    {
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string NI_Number { get; set; }
        public string Payroll_Reference { get; set; }
        public string Date_Of_Birth { get; set; }
        public string Title { get; set; }
        public string OptoutDate { get; set; }
        public string SubmittedDate { get; set; }
        public string MiddleName { get; set; }
        public string CompanyName { get; set; }
        public string Scheme { get; set; }

        public static OptOutRecord FromCSV(string cSVLine)
        {
            OptOutRecord optOutRecord = new OptOutRecord();
            string[] values = cSVLine.Split(',');

            optOutRecord.FirstName = values[0];
            optOutRecord.Surname = values[1];
            optOutRecord.NI_Number = values[2];
            optOutRecord.Payroll_Reference = values[3];
            optOutRecord.Date_Of_Birth = values[4];
            optOutRecord.Title = values[5];
            optOutRecord.OptoutDate = values[6];
            optOutRecord.SubmittedDate = values[7];
            optOutRecord.MiddleName = values[8];
            optOutRecord.CompanyName = values[9];
            optOutRecord.Scheme = values[10];

            return optOutRecord;
        }
    }

    public class FileErrorRecord
    {
        public string PayrollNumber { get; set; }
        public FileErrorAction FileErrorAction { get; set; }

        public static FileErrorRecord FromCSV(string cSVLine)
        {
            FileErrorRecord fileErrorRecord = new FileErrorRecord();
            string[] values = cSVLine.Split(',');

            fileErrorRecord.PayrollNumber = values[0];
            fileErrorRecord.FileErrorAction = (FileErrorAction)Enum.Parse(typeof(FileErrorAction), values[1]);

            return fileErrorRecord;
        }
    }

    public class MappingDefinition
    {
        public List<MappingField> MappingFields { get; set; }
        public List<MappingFilter> MappingFilters { get; set; }
        public string DefinitionFile { get; set; }
        public string CompanyName { get; set; }
        public bool RequiresLevel2File { get; set; }
        public bool RequiresOptOutFile { get; set; }
        public bool CanHaveErrorFile { get; set; }

        public bool HasAdditionalFile { get; set; }

        public List<List<string>> Level2Records { get; set; }
        public DateTime DateLevel2LastLoaded { get; set; }

        public List<OptOutRecord> OptOutRecords { get; set; }
        public DateTime DateOptOutRecordsLastLoaded { get; set; }

        public List<FileErrorRecord> FileErrorRecords { get; set; }
        public DateTime DateErrorRecordsLastLoaded { get; set; }

        public MappingDefinition()
        {
            MappingFields = new List<MappingField>();
            MappingFilters = new List<MappingFilter>();
            Level2Records = new List<List<string>>();
            OptOutRecords = new List<OptOutRecord>();
            FileErrorRecords = new List<FileErrorRecord> { };

            RequiresLevel2File = false;
            RequiresOptOutFile = false;
            CanHaveErrorFile = false;

            DateLevel2LastLoaded = DateTime.MinValue;
        }
    }
}
