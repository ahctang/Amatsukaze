using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using Amatsukaze.ViewModel;
using System.Diagnostics;
using System.Windows;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections.ObjectModel;
using Amatsukaze.HelperClasses;

namespace Amatsukaze.Model
{
    class LibraryMenuModel : ObservableObjectClass
    {        
        public LibraryMenuModel (OptionsObject optionsobject)
        {
            this.optionsobject = optionsobject;
        }        

        #region #objects
        public ObservableCollection<AnimeEntryObject> AnimeLibraryList = new ObservableCollection<AnimeEntryObject>();
        OptionsObject optionsobject;

        #endregion


        #region methods

        //Reads the cache file, but simply creates the cache folder if it doesn't exist
        public void ReadCacheFile()
        {            
            string filepath = optionsobject.CacheFolderpath + @"\CachedData.json";
            if (File.Exists(filepath))
            {
                try
                {
                    string input = File.ReadAllText(filepath);
                    AnimeLibraryList = JsonConvert.DeserializeObject<ObservableCollection<AnimeEntryObject>>(input);
                    this.SendMessagetoGUI(this, new MessageArgs("Cache file read successfully."));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.SendMessagetoGUI(this, new MessageArgs("Cache file not found."));
                FileInfo folderpath = new FileInfo(optionsobject.CacheFolderpath);
                folderpath.Directory.Create();
            }
        }

        //Downloads the image file using the image in the animeentryobject
        private void DownloadAnimeCover(AnimeEntryObject animeentry)
        {
            using (var client = new WebClient())
            {
                try
                {
                    FileInfo folder = new FileInfo(optionsobject.CacheFolderpath + @"Images\");
                    folder.Directory.Create();
                    string path = optionsobject.CacheFolderpath + @"Images\" + animeentry.id.ToString() + ".jpg";
                    client.DownloadFile(animeentry.image, path);                    
                    Console.WriteLine("Downloaded {0}.jpg", animeentry.id.ToString());
                    animeentry.ImagePath = path;

                    string message = "Updated artwork for: ";
                    if (animeentry.english.Length == 0)
                        message += animeentry.title;
                    else
                        message += animeentry.english;

                    this.SendMessagetoGUI(this, new MessageArgs(message));

                    //Wait 2 seconds to stop flooding the server with requests                            
                    System.Threading.Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //Updates the Cache file in the Directory. (Simply replaces the existing cache with a new updated file)
        public void SaveCacheFile(ObservableCollection<AnimeEntryObject> input)
        {
            
            string json = JsonConvert.SerializeObject(input, Newtonsoft.Json.Formatting.Indented);
            try
            {
                string filepath = optionsobject.CacheFolderpath + @"\CachedData.json";

                FileInfo folder = new FileInfo(optionsobject.CacheFolderpath);
                folder.Directory.Create();
                File.WriteAllText(filepath, json);
                Console.WriteLine("Cache file updated");
            }
            catch (Exception exception)
            {
                //FileIO exception
                MessageBox.Show(exception.Message);
            }            
        }

        //Reads the contents of a directory containing XML files with metadata
        public bool ReadXMLDirectory()
        {            
            try
            {
                string[] files = Directory.GetFiles(optionsobject.CacheFolderpath, "*.xml");

                //Check if there are any new xml files
                if (files.Length != 0)
                {
                    //Parse the new ones and add them to the json file.
                    foreach (string filename in files)
                    {
                        string input = File.ReadAllText(filename);

                        //Probably somekind of selector here need to choose which source is used
                        MALDataSource datasource = new MALDataSource();

                        if (datasource.ParseXML(input))
                        {
                            datasource.ContentsDump();
                            AnimeEntryObject animeentry = new AnimeEntryObject(datasource);
                            AnimeLibraryList.Add(animeentry);
                            

                            string message = "Found new anime: ";
                            if (animeentry.english.Length == 0)
                                message += animeentry.title;
                            else
                                message += animeentry.english;

                            this.SendMessagetoGUI(this, new MessageArgs(message));

                            DownloadAnimeCover(animeentry);
                            File.Delete(filename);                                                       
                        }
                        else
                        {
                            Console.WriteLine("Directory Parse failed");
                            return false;
                        }
                    }
                    
                    SaveCacheFile(this.AnimeLibraryList);
                }
                else
                {
                    this.SendMessagetoGUI(this, new MessageArgs("No new anime found."));
                }
                return true;
            }

            catch (Exception ex)
            {
                //Couldn't fill an animeentryobject correctly
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        #endregion

        #region Events/EventHandlers

        public event EventHandler SendMessagetoGUI = delegate { };

        #endregion
    }

    class MessageArgs : EventArgs
    {
        public string Message { get; set; }

        public MessageArgs(string message)
        {
            this.Message = message;
        }
    }
}
