using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Convertor.Respository.ConvertorClasses;
using CsvHelper;

namespace Convertor.Respository.DataLayer
{
    public class CSVConvertorFileManagement
    {

        private static string DefinitionSubDirectory = "csvConvertorDefinition";
        private static string MappingSubDirectory = "csvConvertorMapping";


        public static void CreateWorkingFolder(string workingFolder)
        {
            Directory.CreateDirectory(workingFolder);
        }

        public static string ImportDefinitionFile(string applicationFolder)
        {
            if (ImportCSVConvertorFile(DefinitionSubDirectory, applicationFolder))
            {
                return "File imported";
            }

            return "No file imported";
        }

        public static string ImportMappingFile(string applicationFolder)
        {
            if (ImportCSVConvertorFile(MappingSubDirectory, applicationFolder))
            {
                return "File imported";
            }

            return "No file imported";
        }

        public static List<List<string>> LoadCSVFile(string fileName, bool firstRowHasHeaders)
        {
            // https://github.com/JoshClose/CsvHelper

            List<List<string>> csvData = new List<List<string>>();

            StreamReader textReader = new StreamReader(fileName);
            CsvParser parser = new CsvParser(textReader);
            string[] csvDataRow = null;
            bool dropFirstRow = firstRowHasHeaders;
            try
            {
                while (true)
                {
                    csvDataRow = parser.Read();
                    if (csvDataRow == null)
                    {
                        break;
                    }
                    if (!dropFirstRow)
                    {
                        csvData.Add(csvDataRow.ToList());
                    }
                    dropFirstRow = false;

                }
            }
            catch (Exception e)            {
                
                return null;
            }

            parser.Dispose();
            textReader.Close();

            return csvData;
        }

        private static bool ImportCSVConvertorFile(string targetSubDirectory, string applicationFolder)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension and default file extension 
                fileDialog.DefaultExt = ".xml";
                fileDialog.Filter = "CSV convertor file (*.xml)|*.xml|All files (*.*)|*.*";
                fileDialog.CheckFileExists = true;
                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                Nullable<bool> result = fileDialog.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    // create folder just in case
                    Directory.CreateDirectory(Path.Combine(applicationFolder, targetSubDirectory));

                    File.Copy(fileDialog.FileName, Path.Combine(applicationFolder, targetSubDirectory, fileDialog.SafeFileName), true);
                    return true;
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return false;
        }

        public static bool SaveDefinitionFile(List<MiddlewareField> middlewareFields, string fileName, string applicationFolder, string definitionExtension)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(List<MiddlewareField>));

            Directory.CreateDirectory(Path.Combine(applicationFolder, DefinitionSubDirectory));

            string path = Path.Combine( applicationFolder, DefinitionSubDirectory,  fileName + definitionExtension);
            StreamWriter writer = new StreamWriter(path, false);
            serializer.Serialize(writer, middlewareFields);
            writer.Close();           

            return true;
        }

        public static List<MiddlewareField> ReadDefinitionFile(string fileName, string applicationFolder, string definitionExtension)
        {
            List<MiddlewareField> middlewareFields = new List<MiddlewareField>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<MiddlewareField>));
            string path = Path.Combine(applicationFolder, DefinitionSubDirectory, fileName + definitionExtension);
            StreamReader reader = new StreamReader(path);
            middlewareFields = (List<MiddlewareField>)serializer.Deserialize(reader);

            reader.Close();

            return middlewareFields;

        }

        public static List<string> GetDefinitionFiles(string applicationFolder, string definitionExtension)
        {

            Directory.CreateDirectory(Path.Combine(applicationFolder, DefinitionSubDirectory));

            List<string> definitionFiles = new List<string>();
            foreach (var file in Directory.EnumerateFiles(Path.Combine(applicationFolder, DefinitionSubDirectory), "*" + definitionExtension, SearchOption.AllDirectories))
            {
                definitionFiles.Add(Path.GetFileNameWithoutExtension(file));
            }

            return definitionFiles;
        }

        public static bool DeleteDefinitionFile(string fileName, string applicationFolder, string definitionExtension)
        {
            return DeleteFile(fileName, applicationFolder, DefinitionSubDirectory,definitionExtension);
        }

        public static bool DeleteMappingFile(string fileName, string applicationFolder, string definitionExtension)
        {
            return DeleteFile(fileName, applicationFolder, MappingSubDirectory, definitionExtension);
        }

        private static bool DeleteFile(string fileName, string applicationFolder, string subDirectory, string definitionExtension)
        {
            //  delete file if it exists - trap exception just in case
            try
            {
                string path = Path.Combine(applicationFolder, subDirectory, fileName + definitionExtension);
                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {

                
            }
            return false;
        }

        public static bool SaveMappingFile(MappingDefinition mappingDefinition, string fileName, string applicationFolder, string definitionExtension)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(MappingDefinition));

            Directory.CreateDirectory(Path.Combine(applicationFolder, MappingSubDirectory));

            string path = Path.Combine(applicationFolder, MappingSubDirectory, fileName + definitionExtension);
            StreamWriter writer = new StreamWriter(path, false);
            serializer.Serialize(writer, mappingDefinition);
            writer.Close();

            return true;
        }

        public static MappingDefinition ReadMappingFile(string fileName, string applicationFolder, string definitionExtension)
        {
            MappingDefinition mappingFields = new MappingDefinition();
            XmlSerializer serializer = new XmlSerializer(typeof(MappingDefinition));
            string path = Path.Combine(applicationFolder, MappingSubDirectory, fileName + definitionExtension);
            StreamReader reader = new StreamReader(path);
            mappingFields = (MappingDefinition)serializer.Deserialize(reader);
            mappingFields.HasAdditionalFile = mappingFields.CanHaveErrorFile || mappingFields.RequiresLevel2File || mappingFields.RequiresOptOutFile;
            reader.Close();

            return mappingFields;

        }

        public static List<string> GetMappingFiles(string applicationFolder, string definitionExtension)
        {

            Directory.CreateDirectory(Path.Combine(applicationFolder, MappingSubDirectory));

            List<string> definitionFiles = new List<string>();
            foreach (var file in Directory.EnumerateFiles(Path.Combine(applicationFolder, MappingSubDirectory), "*" + definitionExtension, SearchOption.AllDirectories))
            {
                definitionFiles.Add(Path.GetFileNameWithoutExtension(file));
            }

            return definitionFiles;
        }
    }
}
