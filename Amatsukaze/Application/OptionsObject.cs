using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Windows;
using System.Reflection;

namespace Amatsukaze.ViewModel
{

    //Static object for holding all of the user preferences if they are to be used by other parts of the application
    public class OptionsObject
    {
        public string Themesetting {get; set;}
        public string CacheFolderpath { get; set; }
        
        public bool UseMALDataSource { get; set; }
        public bool UseAniDBDataSource { get; set; }

        //For AniDB Api
        public string AniDBImageURL { get; set; } 
        public string AniDBXMLQueryURL { get; set; }
        public string AniDBIDRetrieverURL { get; set; }
        
        //For MAL Api
        public string MALXMLQueryURL { get; set; }      
        public string MALLogin { get; set; }
        public string MALPassword { get; set; }

        //Sets the options object to default values.
        public void SetDefaults()
        {
            this.Themesetting = "Amatsukaze";
            this.CacheFolderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Cache\";
            this.UseMALDataSource = true;
            this.UseAniDBDataSource = false;
            this.AniDBImageURL = "https://img7.anidb.net/pics/anime/";
            this.AniDBXMLQueryURL = "http://api.anidb.net:9001/httpapi?request=anime&client=Amatsukaze&clientver=1&protover=1&aid=";
            this.AniDBIDRetrieverURL = "http://anisearch.outrance.pl/?task=search&query=";
            this.MALXMLQueryURL = "http://myanimelist.net/api/anime/search.xml?q=";
        }

        //The Model class primarily deals with the IO related to saving the user preferences to a JSON file on disk
        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
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

        [Conditional("DEBUG")]
        public void OptionsDump()
        {
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                Console.WriteLine("{0}; {1}", property.Name, property.GetValue(this, null));
            }
        }
    }
}
