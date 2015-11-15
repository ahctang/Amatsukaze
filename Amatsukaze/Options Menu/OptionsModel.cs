using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Amatsukaze.ViewModel;
using System.Windows;
using System.Diagnostics;
using System.Reflection;

namespace Amatsukaze.Model
{     
    class OptionsModel
    {
        //The Model class primarily deals with the IO related to saving the user preferences to a JSON file on disk
        public void SaveOptionsFile(OptionsObject optionsobject)
        {
            string json = JsonConvert.SerializeObject(optionsobject, Formatting.Indented);
            try
            {
                string folderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Preferences\";
                string filepath = folderpath + @"\UserPreferences.json";

                FileInfo folder = new FileInfo(folderpath);
                folder.Directory.Create();
                File.WriteAllText(filepath, json);
            }
            catch (Exception exception)
            {
                //FileIO exception
                MessageBox.Show(exception.Message);
            }
        }

        //Reads the options file into memory/ creates a new one if it doesn't exist
        public void ReadOptionsFile(ref OptionsObject optionsobject)
        {
            string folderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Preferences\";
            string filepath = folderpath + @"\UserPreferences.json";

            if (File.Exists(filepath))
            {
                string input = File.ReadAllText(filepath);
                optionsobject = JsonConvert.DeserializeObject<OptionsObject>(input);
                OptionsDump(optionsobject);
            }
            else
            {
                optionsobject.SetDefaults();
                SaveOptionsFile(optionsobject);
                OptionsDump(optionsobject);
            }                       
        }

        

        [Conditional("DEBUG")]
        public void OptionsDump(OptionsObject optionsobject)
        {
            Type type = optionsobject.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                Console.WriteLine("{0}; {1}", property.Name, property.GetValue(optionsobject, null));
            }
        }


    }
}
