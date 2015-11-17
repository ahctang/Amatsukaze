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
        //Reads the options file into memory/ creates a new one if it doesn't exist
        public void ReadOptionsFile(ref OptionsObject optionsObject)
        {
            string folderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Preferences\";
            string filepath = folderpath + @"\UserPreferences.json";

            if (File.Exists(filepath))
            {
                string input = File.ReadAllText(filepath);
                optionsObject = JsonConvert.DeserializeObject<OptionsObject>(input);
                optionsObject.OptionsDump();                
            }
            else
            {
                optionsObject.SetDefaults();
                optionsObject.Save();
                optionsObject.OptionsDump();
            }
        }
    }
}
