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
using System.Threading;

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
        private void DownloadAnimeCoverArt(List<AnimeEntryObject> NewAnimeBuffer)
        {
            //Subscribe the event handlers
            AnimeCoverResourceReady += new EventHandler(OnAnimeCoverResourceReady);

            using (var client = new WebClient())
            {
                foreach (AnimeEntryObject animeentry in NewAnimeBuffer)
                {
                    try
                    {
                        if (animeentry.Sources.Contains("MAL"))
                        {
                            FileInfo folder = new FileInfo(optionsobject.CacheFolderpath + @"Images\");
                            folder.Directory.Create();
                            string path = optionsobject.CacheFolderpath + @"Images\" + animeentry.id.ToString() + ".jpg";
                            client.DownloadFile(animeentry.image, path);

                            Console.WriteLine("Downloaded {0}.jpg", animeentry.id.ToString());
                            animeentry.ImagePath = path;

                            /*string message = "Library: Updated artwork (MAL) for: ";
                            if (animeentry.english.Length == 0)
                                message += animeentry.title;
                            else
                                message += animeentry.english;

                            this.SendMessagetoGUI(this, new MessageArgs(message));*/

                            //Wait 2 seconds to stop flooding the server with requests                            
                            Thread.Sleep(2000);

                            //Raise the event to indicate the stuff is ready
                            AnimeCoverResourceReady(this, new AnimeCoverArgs(animeentry));
                        }

                        if (animeentry.Sources.Contains("AniDB"))
                        {
                            FileInfo folder = new FileInfo(optionsobject.CacheFolderpath + @"Images\");
                            folder.Directory.Create();
                            string path = optionsobject.CacheFolderpath + @"Images\AniDB-" + animeentry.id.ToString() + ".jpg";
                            client.DownloadFile(optionsobject.AniDBImageURL + animeentry.image, path);

                            Console.WriteLine("Downloaded {0}.jpg", animeentry.id.ToString());
                            animeentry.ImagePath = path;

                            /*string message = "Library: Updated artwork (AniDB) for: ";
                            if (animeentry.english.Length == 0)
                                message += animeentry.title;
                            else
                                message += animeentry.english;

                            this.SendMessagetoGUI(this, new MessageArgs(message));*/

                            //Wait 2 seconds to stop flooding the server with requests                            
                            Thread.Sleep(2000);
                            //Raise the event to indicate the stuff is ready
                            AnimeCoverResourceReady(this, new AnimeCoverArgs(animeentry));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        /*string message = "Library: Could not retrieve artwork for: ";
                        if (animeentry.english.Length == 0)
                            message += animeentry.title;
                        else
                            message += animeentry.english;

                        this.SendMessagetoGUI(this, new MessageArgs(message));*/
                        AnimeCoverResourceReady(this, new AnimeCoverArgs(animeentry, ""));
                    }
                }
            }

            //Unsubscribe the event handlers
            AnimeCoverResourceReady -= OnAnimeCoverResourceReady;

            return;
        }

        private void DownloadAnimeCharacterArt(List<AnimeEntryObject> NewAnimeBuffer)
        {
            //Subscribe the event handlers
            AnimeCharacterResourceReady += new EventHandler(OnAnimeCharacterResourceReady);

            using (var client = new WebClient())
            {
                foreach (AnimeEntryObject animeentry in NewAnimeBuffer)
                {
                    try
                    {
                        if (animeentry.Characters.Count == 0) continue;

                        FileInfo folder = new FileInfo(optionsobject.CacheFolderpath + @"Images\" + animeentry.id.ToString() + @"\");
                        folder.Directory.Create();
                        foreach (AnimeCharacter character in animeentry.Characters)
                        {
                            string path = optionsobject.CacheFolderpath + @"Images\" + animeentry.id.ToString() + @"\" + character.CharacterName + ".jpg";
                            client.DownloadFile(optionsobject.AniDBImageURL + character.CharacterPicture, path);

                            character.PicturePath = path;

                            Console.WriteLine("Downloaded {0}.jpg", animeentry.id.ToString());
                            Thread.Sleep(2000);
                        }

                        AnimeCharacterResourceReady(this, new AnimeCharacterArgs(animeentry, animeentry.Characters));

                        /*string message = "Library: Updated character artwork (AniDB) for: ";
                        if (animeentry.english.Length == 0)
                            message += animeentry.title;
                        else
                            message += animeentry.english;

                        this.SendMessagetoGUI(this, new MessageArgs(message));*/

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        /*string message = "Library: Could not retrieve character artwork for: ";
                        if (animeentry.english.Length == 0)
                            message += animeentry.title;
                        else
                            message += animeentry.english;

                        this.SendMessagetoGUI(this, new MessageArgs(message));*/
                    }
                }
            }

            //Unsubscribe the event handlers
            AnimeCharacterResourceReady -= OnAnimeCharacterResourceReady;
        }

        //Updates the Cache file in the Directory. (Simply replaces the existing cache with a new updated file)
        public void SaveCacheFile(ObservableCollection<AnimeEntryObject> input)
        {
            lock (AnimeLibraryList)
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
                List<AnimeEntryObject> NewAnimeBuffer = new List<AnimeEntryObject>();

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
                                    if (File.Exists(filepath) && File.ReadAllText(filepath) != "null")
                                    {
                                        string MalCacheJson = File.ReadAllText(filepath);
                                        MALDataCache = JsonConvert.DeserializeObject<List<MALDataSource>>(MalCacheJson);
                                    }
                                    else
                                    {
                                        MALDataCache = new List<MALDataSource>();
                                    }
                                }

                                //Check the MALDataCache for a duplicate
                                var MALQuery = MALDataCache.FirstOrDefault(i => i.title.ToLower() == MALdatasource.title.ToLower());

                                //Add to the datacache if the info doesn't already exist
                                if (MALQuery == null)
                                {
                                    MALDataCache.Add(MALdatasource);
                                }

                                //Check if the info already exists in AnimeLibraryList
                                var AnimeLibraryListQuery = AnimeLibraryList.FirstOrDefault(i => i.title.ToLower() == MALdatasource.title.ToLower()
                                                                                            || (i.english != "" && i.english.ToLower() == MALdatasource.english.ToLower()));

                                if (AnimeLibraryListQuery != null)
                                {
                                    if (AnimeLibraryListQuery.Sources.Contains("MAL") == true)
                                    {
                                        //The entry is a duplicate, so the file should be deleted and skipped.
                                        File.Delete(filename);                                        
                                    }
                                    else
                                    {
                                        //The entry exists, but no data from MAL is present. Merge the unset data fields
                                        AnimeLibraryListQuery.MergeInfo(MALdatasource, this.optionsobject);

                                        string messagebuffer = "Library: Updated info (MAL): ";
                                        if (MALdatasource.english.Length == 0)
                                            messagebuffer += MALdatasource.title;
                                        else
                                            messagebuffer += MALdatasource.english;

                                        this.SendMessagetoGUI(this, new MessageArgs(messagebuffer));
                                        File.Delete(filename);                                                                                
                                    }
                                }
                                //So the entry is not in the AnimeLibraryList. Check if the entry exists in the existing buffer
                                else if (AnimeLibraryListQuery == null)
                                {
                                    var NewAnimeBufferQuery = NewAnimeBuffer.FirstOrDefault(i => i.title.ToLower() == MALdatasource.title.ToLower()
                                                                                                || (i.english != "" &&i.english.ToLower() == MALdatasource.english.ToLower()));

                                    //It already exists!
                                    if (NewAnimeBufferQuery != null)
                                    {
                                        if (NewAnimeBufferQuery.Sources.Contains("MAL") == true)
                                        {
                                            //The entry is a duplicate, so the file should be deleted and skipped.
                                            File.Delete(filename);                                            
                                        }
                                        else
                                        {
                                            //The entry exists, but no data from MAL is present. Merge the unset data fields
                                            NewAnimeBufferQuery.MergeInfo(MALdatasource, this.optionsobject);

                                            string messagebuffer = "Library: Updated info (MAL): ";
                                            if (MALdatasource.english.Length == 0)
                                                messagebuffer += MALdatasource.title;
                                            else
                                                messagebuffer += MALdatasource.english;

                                            this.SendMessagetoGUI(this, new MessageArgs(messagebuffer));
                                            File.Delete(filename);                                            
                                        }
                                    }
                                    else //Doesn't exist!
                                    {
                                        //Add the info as a new object for the new anime buffer
                                        NewAnimeBuffer.Add(new AnimeEntryObject(MALdatasource));
                                        string message = "Library: Found new anime(MAL): ";
                                        if (MALdatasource.english.Length == 0)
                                            message += MALdatasource.title;
                                        else
                                            message += MALdatasource.english;

                                        this.SendMessagetoGUI(this, new MessageArgs(message));
                                        File.Delete(filename);
                                    }
                                }
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
                                    if (File.Exists(filepath) && File.ReadAllText(filepath) != "null")
                                    {
                                        string AniDBCacheJson = File.ReadAllText(filepath);
                                        AniDBDataCache = JsonConvert.DeserializeObject<List<AniDBDataSource>>(AniDBCacheJson);
                                    }
                                    else
                                    {
                                        AniDBDataCache = new List<AniDBDataSource>();
                                    }
                                }

                                //Check the AniDBDataCache for a duplicate
                                var AniDBQuery = AniDBDataCache.FirstOrDefault(i => i.title.ToLower() == AniDBdatasource.title.ToLower());

                                //Add to the datacache if the info doesn't already exist
                                if (AniDBQuery == null)
                                {
                                    AniDBDataCache.Add(AniDBdatasource);
                                }

                                //Check if the info already exists in AnimeLibraryList
                                var AnimeLibraryListQuery = AnimeLibraryList.FirstOrDefault(i => i.title.ToLower() == AniDBdatasource.title.ToLower()
                                                                                            || (i.english != "" && i.english.ToLower() == AniDBdatasource.english.ToLower()));

                                if (AnimeLibraryListQuery != null)
                                {
                                    if (AnimeLibraryListQuery.Sources.Contains("AniDB") == true)
                                    {
                                        //The entry is a duplicate, so the file should be deleted and skipped.
                                        File.Delete(filename);                                        
                                    }
                                    else
                                    {
                                        //The entry exists, but no data from AniDB is present. Merge the unset data fields
                                        AnimeLibraryListQuery.MergeInfo(AniDBdatasource, this.optionsobject);

                                        string messagebuffer = "Library: Updated info (AniDB): ";
                                        if (AniDBdatasource.english.Length == 0)
                                            messagebuffer += AniDBdatasource.title;
                                        else
                                            messagebuffer += AniDBdatasource.english;

                                        this.SendMessagetoGUI(this, new MessageArgs(messagebuffer));
                                        File.Delete(filename);                                                                              
                                    }
                                }
                                //So the entry is not in the AnimeLibraryList. Check if the entry exists in the existing buffer
                                else if (AnimeLibraryListQuery == null)
                                {
                                    var NewAnimeBufferQuery = NewAnimeBuffer.FirstOrDefault(i => i.title.ToLower() == AniDBdatasource.title.ToLower()
                                                                                            || i.english.ToLower() == AniDBdatasource.english.ToLower());

                                    //It already exists!
                                    if (NewAnimeBufferQuery != null)
                                    {
                                        if (NewAnimeBufferQuery.Sources.Contains("AniDB") == true)
                                        {
                                            //The entry is a duplicate, so the file should be deleted and skipped.
                                            File.Delete(filename);                                            
                                        }
                                        else
                                        {
                                            //The entry exists, but no data from AniDB is present. Merge the unset data fields
                                            NewAnimeBufferQuery.MergeInfo(AniDBdatasource, this.optionsobject);

                                            string messagebuffer = "Library: Updated info (AniDB): ";
                                            if (AniDBdatasource.english.Length == 0)
                                                messagebuffer += AniDBdatasource.title;
                                            else
                                                messagebuffer += AniDBdatasource.english;

                                            this.SendMessagetoGUI(this, new MessageArgs(messagebuffer));
                                            File.Delete(filename);                                                                                                                               
                                        }
                                    }
                                    else //Doesn't exist!
                                    {
                                        //Add the info as a new object for the new anime buffer
                                        NewAnimeBuffer.Add(new AnimeEntryObject(AniDBdatasource));
                                        string message = "Library: Found new anime(AniDB): ";
                                        if (AniDBdatasource.english.Length == 0)
                                            message += AniDBdatasource.title;
                                        else
                                            message += AniDBdatasource.english;

                                        this.SendMessagetoGUI(this, new MessageArgs(message));
                                        File.Delete(filename);
                                    }
                                }
                            }
                        }
                    }

                    //Save both updated caches (only if they were initialized)
                    if(MALDataCache != null) SaveCacheFile(MALDataCache);
                    if(AniDBDataCache != null) SaveCacheFile(AniDBDataCache);

                    //Add the buffer list to AnimeLibraryList
                    foreach (AnimeEntryObject anime in NewAnimeBuffer)
                    {
                        this.AnimeLibraryList.Add(anime);

                        string message = "Library: Added new anime (";
                        for (int i = 0; i < anime.Sources.Count; i++)
                        {
                            if (i == anime.Sources.Count - 1)
                            {
                                message += anime.Sources[i] + ")";
                            }
                            else
                            {
                                message += anime.Sources[i] + "+";
                            }
                        }

                        message += ": ";

                        if (anime.english.Length == 0)
                            message += anime.title;
                        else
                            message += anime.english;

                        this.SendMessagetoGUI(this, new MessageArgs(message));

                        //Save the AnimeLibraryList cache file
                        SaveCacheFile(AnimeLibraryList);
                    }

                    //Start the tasks to retrieve the resources from the internet
                    List<Task> TaskList = new List<Task>();

                    TaskList.Add(Task.Run(() => DownloadAnimeCoverArt(NewAnimeBuffer)));
                    TaskList.Add(Task.Run(() => DownloadAnimeCharacterArt(NewAnimeBuffer)));

                    await Task.WhenAll(TaskList);
                }
                else
                {
                    this.SendMessagetoGUI(this, new MessageArgs("Library: Search Ended. No new anime found."));
                }
                IsRefreshInProgess = false;

                string message2 = "Library: Directory scan finished. " + NewAnimeBuffer.Count.ToString() + " new anime added.";

                this.SendMessagetoGUI(this, new MessageArgs(message2));
                return true;
            }

            catch (Exception ex)
            {
                //Couldn't fill an animeentryobject correctly
                Console.WriteLine(ex.Message);
                this.SendMessagetoGUI(this, new MessageArgs("Library: Refresh operation failed."));
                IsRefreshInProgess = false;
                return false;
            }
        }


        //Searches for the animeentryobject on the internets and checks against existing objects again.
        //Returns true if object was updated and false if object was not updated or exception thrown.
        public bool RetrySearch(AnimeEntryObject input)
        {
            //This part should feed it back into a method that queries the web apis
            //NOT YET IMPLEMENTED

            //This part checks it against existing entries.
            var AnimeLibraryListQuery = AnimeLibraryList.FirstOrDefault(i => i.title.ToLower() == input.title.ToLower()
                                                                        || i.english.ToLower() == input.english.ToLower());

            //A match has been found!
            if(AnimeLibraryList != null)
            {
                //AnimeLibraryListQuery.MergeInfo(input, optionsobject); //How do I merge this??

                //Delete the input entry
                AnimeLibraryList.RemoveAt(AnimeLibraryList.IndexOf(input));

                return true;                
            }

            return false;
        }
        #endregion

        #region Events/EventHandlers

        public event EventHandler SendMessagetoGUI = delegate { };
        public event EventHandler AnimeCoverResourceReady;
        public event EventHandler AnimeCharacterResourceReady;

        public void OnAnimeCoverResourceReady(object sender, EventArgs e)
        {
            if (e != null)
            {
                var input = e as AnimeCoverArgs;

                if (input.ImagePath.Length != 0)
                {
                    var AnimeLibraryListQuery = AnimeLibraryList.FirstOrDefault(i => i.title.ToLower() == input.Title.ToLower());

                    if (AnimeLibraryList != null)
                    {
                        AnimeLibraryListQuery.MergeAnimeCover(input.ImagePath);

                        string message = "Library: Updated artwork for: ";
                        if (input.EnglishTitle.Length == 0)
                            message += input.Title;
                        else
                            message += input.EnglishTitle;

                        this.SendMessagetoGUI(this, new MessageArgs(message));

                        //Save the AnimeLibraryList cache file
                        SaveCacheFile(AnimeLibraryList);
                    }
                }
                else
                {
                    string message = "Library: Could not retrieve artwork for: ";
                    if (input.EnglishTitle.Length == 0)
                        message += input.Title;
                    else
                        message += input.EnglishTitle;

                    this.SendMessagetoGUI(this, new MessageArgs(message));
                }
            }
        }

        public void OnAnimeCharacterResourceReady(object sender, EventArgs e)
        {
            if (e != null)
            {
                var input = e as AnimeCharacterArgs;

                if (input.AnimeCharacterList.Count > 0)
                {
                    var AnimeLibraryListQuery = AnimeLibraryList.FirstOrDefault(i => i.title.ToLower() == input.Title.ToLower());

                    if (AnimeLibraryList != null)
                    {
                        AnimeLibraryListQuery.MergeCharacters(input.AnimeCharacterList);

                        string message = "Library: Updated character artwork (AniDB) for: ";
                        if (input.EnglishTitle.Length == 0)
                            message += input.Title;
                        else
                            message += input.EnglishTitle;

                        this.SendMessagetoGUI(this, new MessageArgs(message));

                        //Save the AnimeLibraryList cache file
                        SaveCacheFile(AnimeLibraryList);
                    }
                }
                else
                {
                    string message = "Library: Could not retrieve character artwork for: ";
                    if (input.EnglishTitle.Length == 0)
                        message += input.Title;
                    else
                        message += input.EnglishTitle;

                    this.SendMessagetoGUI(this, new MessageArgs(message));
                }
            }

        }

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

    class AnimeCoverArgs : EventArgs
    {
        public string Title { get; set; }
        public string EnglishTitle { get; set; }
        public string ImagePath { get; set; }

        public AnimeCoverArgs(AnimeEntryObject input)
        {
            this.Title = input.title;
            this.EnglishTitle = input.english;
            this.ImagePath = input.ImagePath;
        }

        public AnimeCoverArgs(AnimeEntryObject input, string ImagePath)
        {
            this.Title = input.title;
            this.EnglishTitle = input.english;
            this.ImagePath = ImagePath;
        }
    }

    class AnimeCharacterArgs : EventArgs
    {
        public string Title { get; set; }
        public string EnglishTitle { get; set; }
        public List<AnimeCharacter> AnimeCharacterList { get; set; }

        public AnimeCharacterArgs(AnimeEntryObject input)
        {
            this.Title = input.title;
            this.EnglishTitle = input.english;
            this.AnimeCharacterList = input.Characters;
        }

        public AnimeCharacterArgs(AnimeEntryObject input, List<AnimeCharacter> CharacterList)
        {
            this.Title = input.title;
            this.EnglishTitle = input.english;
            this.AnimeCharacterList = CharacterList;
        }
    }


}
