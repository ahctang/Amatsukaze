using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Amatsukaze.ViewModel;
using System.Windows;
using System.Net;
using System.Collections.ObjectModel;
using Amatsukaze.HelperClasses;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Amatsukaze.Model
{
    class LibraryMenuModel : ObservableObjectClass
    {
        public LibraryMenuModel(OptionsObject optionsobject)
        {
            this.optionsobject = optionsobject;            
        }

        #region #objects
        public ObservableCollection<AnimeEntryObject> AnimeLibraryList = new ObservableCollection<AnimeEntryObject>();
        public List<MALDataSource> MALDataCache;
        public List<AniDBDataSource> AniDBDataCache;       
        OptionsObject optionsobject;

        #endregion

        #region Properties

        //Bool to block off refresh so it can only be executed in one thread at a time
        public bool IsRefreshInProgess { get; set; }

        #endregion

        #region Methods

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
                    this.SendMessagetoGUI(this, new MessageArgs("Library: Cache file read successfully."));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                this.SendMessagetoGUI(this, new MessageArgs("Library: Cache file not found."));
                FileInfo folderpath = new FileInfo(optionsobject.CacheFolderpath);
                folderpath.Directory.Create();
            }
        }

        //Downloads the image file using the image in the animeentryobject
        private async Task<bool> DownloadAnimeCoverAsync(AnimeEntryObject animeentry, string Source)
        {
            using (var client = new WebClient())
            {
                try
                {
                    if (Source == "MAL")
                    {
                        FileInfo folder = new FileInfo(optionsobject.CacheFolderpath + @"Images\");
                        folder.Directory.Create();
                        string path = optionsobject.CacheFolderpath + @"Images\" + animeentry.id.ToString() + ".jpg";
                        client.DownloadFile(animeentry.image, path);

                        Console.WriteLine("Downloaded {0}.jpg", animeentry.id.ToString());
                        animeentry.ImagePath = path;

                        string message = "Library: Updated artwork (MAL) for: ";
                        if (animeentry.english.Length == 0)
                            message += animeentry.title;
                        else
                            message += animeentry.english;

                        this.SendMessagetoGUI(this, new MessageArgs(message));

                        //Wait 2 seconds to stop flooding the server with requests                            
                        await Task.Delay(2000);

                        return true;
                    }

                    else if (Source == "AniDB")
                    {
                        FileInfo folder = new FileInfo(optionsobject.CacheFolderpath + @"Images\");
                        folder.Directory.Create();
                        string path = optionsobject.CacheFolderpath + @"Images\AniDB-" + animeentry.id.ToString() + ".jpg";
                        client.DownloadFile(optionsobject.AniDBImageURL + animeentry.image, path);

                        Console.WriteLine("Downloaded {0}.jpg", animeentry.id.ToString());
                        animeentry.ImagePath = path;

                        string message = "Library: Updated artwork (AniDB) for: ";
                        if (animeentry.english.Length == 0)
                            message += animeentry.title;
                        else
                            message += animeentry.english;

                        this.SendMessagetoGUI(this, new MessageArgs(message));

                        //Wait 2 seconds to stop flooding the server with requests                            
                        await Task.Delay(2000);

                        return true;
                    }

                    return false;                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    string message = "Library: Could not retrieve artwork for: ";
                    if (animeentry.english.Length == 0)
                        message += animeentry.title;
                    else
                        message += animeentry.english;

                    this.SendMessagetoGUI(this, new MessageArgs(message));
                    return false;
                }
            }
        }

        private async Task<bool> DownloadAnimeCharacterArt(AnimeEntryObject animeentry)
        {
            using (var client = new WebClient())
            {
                try
                {
                    if (animeentry.Characters.Count == 0) return false;
                    FileInfo folder = new FileInfo(optionsobject.CacheFolderpath + @"Images\" + animeentry.id.ToString() + @"\");
                    folder.Directory.Create();
                    foreach (AnimeCharacter character in animeentry.Characters)
                    {
                        string path = optionsobject.CacheFolderpath + @"Images\" + animeentry.id.ToString() + @"\" + character.CharacterName + ".jpg";
                        client.DownloadFile(optionsobject.AniDBImageURL + character.CharacterPicture, path);

                        character.PicturePath = path;

                        await Task.Delay(2000);
                    }                   

                    Console.WriteLine("Downloaded {0}.jpg", animeentry.id.ToString());                    

                    string message = "Library: Updated character artwork (AniDB) for: ";
                    if (animeentry.english.Length == 0)
                        message += animeentry.title;
                    else
                        message += animeentry.english;

                    this.SendMessagetoGUI(this, new MessageArgs(message));                                      

                    return true;                                                                        
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    string message = "Library: Could not retrieve character artwork for: ";
                    if (animeentry.english.Length == 0)
                        message += animeentry.title;
                    else
                        message += animeentry.english;

                    this.SendMessagetoGUI(this, new MessageArgs(message));
                    return false;
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

        public void SaveCacheFile(List<MALDataSource> input)
        {

            string json = JsonConvert.SerializeObject(input, Newtonsoft.Json.Formatting.Indented);
            try
            {
                string filepath = optionsobject.CacheFolderpath + @"\MalDataCache.json";

                FileInfo folder = new FileInfo(optionsobject.CacheFolderpath);
                folder.Directory.Create();
                File.WriteAllText(filepath, json);
                Console.WriteLine("MAL Cache file updated");
            }
            catch (Exception exception)
            {
                //FileIO exception
                MessageBox.Show(exception.Message);
            }
        }

        public void SaveCacheFile(List<AniDBDataSource> input)
        {

            string json = JsonConvert.SerializeObject(input, Newtonsoft.Json.Formatting.Indented);
            try
            {
                string filepath = optionsobject.CacheFolderpath + @"\AniDBDataCache.json";

                FileInfo folder = new FileInfo(optionsobject.CacheFolderpath);
                folder.Directory.Create();
                File.WriteAllText(filepath, json);
                Console.WriteLine("AniDB Cache file updated");
            }
            catch (Exception exception)
            {
                //FileIO exception
                MessageBox.Show(exception.Message);
            }
        }

        //Reads the contents of a directory containing XML files with metadata
        public async Task<bool> ReadXMLDirectoryAsync()
        {
            if (IsRefreshInProgess == true) return false;
            try
            {
                IsRefreshInProgess = true;
                string[] files = Directory.GetFiles(optionsobject.CacheFolderpath, "*.xml");

                //Buffer lists to make it easier to only add new objects to AnimeLibraryList
                List<MALDataSource> MALbuffer = new List<MALDataSource>();
                List<AniDBDataSource> AniDBbuffer = new List<AniDBDataSource>();

                //Check if there are any new xml files
                if (files.Length != 0)
                {                   
                    //Parse the new ones and add them to the json file.
                    foreach (string filename in files)
                   {
                        string input = File.ReadAllText(filename);

                        //MAL XML is labelled MAL-XXXX.xml. AniDB XML title starts with "AniDB-XXXX.xml"                                               
                        string buffer = Path.GetFileName(filename);
                        string sourceName = buffer.Substring(0, buffer.IndexOf("-"));
                        
                        if (sourceName == "MAL" && optionsobject.UseMALDataSource == true)
                        {
                            MALDataSource MALdatasource = new MALDataSource();                           

                            if (XMLParsers.MALParseXML(input, MALdatasource))
                            {
                                MALdatasource.ContentsDump();

                                //Initialize the MAL cache file
                                if (MALDataCache == null)
                                {
                                    string filepath = optionsobject.CacheFolderpath + @"\MalDataCache.json";
                                    if (File.Exists(filepath))
                                    {
                                        string MalCacheJson = File.ReadAllText(filepath);
                                        MALDataCache = JsonConvert.DeserializeObject<List<MALDataSource>>(MalCacheJson);
                                    }
                                    else
                                    {
                                        MALDataCache = new List<MALDataSource>();
                                    }
                                }

                                //Add to the new collection
                                MALDataCache.Add(MALdatasource);

                                //Add to the buffer
                                MALbuffer.Add(MALdatasource);

                                string message = "Library: Found new anime(MAL): ";
                                if (MALdatasource.english.Length == 0)
                                    message += MALdatasource.title;
                                else
                                    message += MALdatasource.english;

                                this.SendMessagetoGUI(this, new MessageArgs(message));
                                File.Delete(filename);                                
                            }
                            else
                            {
                                this.SendMessagetoGUI(this, new MessageArgs("Library: Directory parse failed."));
                                return false;
                            }
                        }
                        else if (sourceName == "AniDB" && optionsobject.UseAniDBDataSource == true)
                        {
                            AniDBDataSource AniDBdatasource = new AniDBDataSource();

                            if (XMLParsers.AniDBParseXML(input, AniDBdatasource))
                            {
                                AniDBdatasource.ContentsDump();

                                //Initialize the AniDB cache file
                                if (AniDBDataCache == null)
                                {
                                    string filepath = optionsobject.CacheFolderpath + @"\AniDBDataCache.json";
                                    if (File.Exists(filepath))
                                    {
                                        string AniDBCacheJson = File.ReadAllText(filepath);
                                        AniDBDataCache = JsonConvert.DeserializeObject<List<AniDBDataSource>>(AniDBCacheJson);
                                    }
                                    else
                                    {
                                        AniDBDataCache = new List<AniDBDataSource>();
                                    }
                                }

                                //Add to the new collection
                                AniDBDataCache.Add(AniDBdatasource);

                                //Add to the buffer
                                AniDBbuffer.Add(AniDBdatasource);

                                string message = "Library: Found new anime(AniDB): ";
                                if (AniDBdatasource.EnglishTitle.Length == 0)
                                    message += AniDBdatasource.Title;
                                else
                                    message += AniDBdatasource.EnglishTitle;

                                this.SendMessagetoGUI(this, new MessageArgs(message));
                                File.Delete(filename);
                            }
                        }                                             
                    }

                    //Save both updated caches
                    SaveCacheFile(MALDataCache);
                    SaveCacheFile(AniDBDataCache);

                    //Check for duplicates
                    //Decide merge rules
                    //Merge objects and add to animelibrarylist

                    //Send list of added objects to functions that collect resources
                    //Raise event/event args when a particular resource is ready
                    //Resources should be updated async

                    //Now to construct the correct animeentry object
                    if (optionsobject.UseMALDataSource == true && optionsobject.UseAniDBDataSource == true)
                    {
                        foreach (MALDataSource MAL in MALDataCache)
                        {
                            var AniDBQuery = AniDBbuffer.FirstOrDefault(i => i.Title.ToLower() == MAL.title.ToLower());

                            if (AniDBQuery != null)
                            {
                                AnimeEntryObject animeentry = new AnimeEntryObject(MAL, AniDBQuery);

                                bool result = await DownloadAnimeCoverAsync(animeentry, "MAL");

                                //There needs to be some sort of check for duplicates before adding
                                AnimeLibraryList.Add(animeentry);                              

                                string message = "Library: Added new anime(MAL & AniDB): ";
                                if (animeentry.english.Length == 0)
                                    message += animeentry.title;
                                else
                                    message += animeentry.english;

                                this.SendMessagetoGUI(this, new MessageArgs(message));

                               bool result2 = await DownloadAnimeCharacterArt(animeentry);                                
                            }
                            else
                            {
                                AnimeEntryObject animeentry = new AnimeEntryObject(MAL);
                                bool result = await DownloadAnimeCoverAsync(animeentry, "MAL");

                                //There needs to be some sort of check for duplicates before adding
                                AnimeLibraryList.Add(animeentry);

                                string message = "Library: Added new anime(MAL): ";
                                if (animeentry.english.Length == 0)
                                    message += animeentry.title;
                                else
                                    message += animeentry.english;

                                this.SendMessagetoGUI(this, new MessageArgs(message));
                            }                                                   
                        }                                                                    
                    }
                    else if (optionsobject.UseMALDataSource == true)
                    {
                        foreach (MALDataSource MAL in MALDataCache)
                        {
                            AnimeEntryObject animeentry = new AnimeEntryObject(MAL);
                            bool result = await DownloadAnimeCoverAsync(animeentry, "MAL");

                            //There needs to be some sort of check for duplicates before adding
                            AnimeLibraryList.Add(animeentry);

                            string message = "Library: Added new anime(MAL): ";
                            if (animeentry.english.Length == 0)
                                message += animeentry.title;
                            else
                                message += animeentry.english;

                            this.SendMessagetoGUI(this, new MessageArgs(message));
                        }                                                
                    }
                    else if (optionsobject.UseAniDBDataSource == true)
                    {
                        foreach (AniDBDataSource AniDB in AniDBDataCache)
                        {
                            AnimeEntryObject animeentry = new AnimeEntryObject(AniDB);
                            bool result = await DownloadAnimeCoverAsync(animeentry, "AniDB");                            

                            //There needs to be some sort of check for duplicates before adding
                            AnimeLibraryList.Add(animeentry);

                            string message = "Library: Added new anime(AniDB): ";
                            if (animeentry.english.Length == 0)
                                message += animeentry.title;
                            else
                                message += animeentry.english;

                            this.SendMessagetoGUI(this, new MessageArgs(message));

                            bool result2 = await DownloadAnimeCharacterArt(animeentry);
                        }                       
                    }
                    this.SendMessagetoGUI(this, new MessageArgs("Library: Directory parse complete."));
                    SaveCacheFile(this.AnimeLibraryList);
                }
                else
                {
                    this.SendMessagetoGUI(this, new MessageArgs("Library: Search Ended. No new anime found."));
                }
                IsRefreshInProgess = false;
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
        public event EventHandler AnimeCoverResourceReady;
        public event EventHandler AnimeCharacterResourceReady;

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
