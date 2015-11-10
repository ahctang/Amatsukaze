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
using System.Security;
using System.Text.RegularExpressions;
using System.Net;

namespace Amatsukaze.Model
{
    class LibraryMenuModel
    {
        #region #objects
        public List<AnimeEntryObject> AnimeLibraryList = new List<AnimeEntryObject>();

        #endregion


        #region methods

        //Reads the cache file, but returns nothing if the cache hasn't been created yet
        public void ReadCacheFile()
        {
            string folderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Cache\";
            string filepath = folderpath + @"\CachedData.json";
            if (File.Exists(filepath))
            {
                try
                {
                    string input = File.ReadAllText(filepath);
                    AnimeLibraryList = JsonConvert.DeserializeObject<List<AnimeEntryObject>>(input);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else return;
        }

        //Downloads the image file using the imageURL in the animeentryobject
        public void DownloadAnimeCover(AnimeEntryObject animeentry)
        {
            using (var client = new WebClient())
            {
                try
                {
                    string path = Path.GetDirectoryName(
                            Process.GetCurrentProcess().MainModule.FileName) + @"\Cache\" + animeentry.id.ToString() + ".jpg";
                    client.DownloadFile(animeentry.imageURL, path);                    
                    Console.WriteLine("Downloaded {0}.jpg", animeentry.id.ToString());
                    animeentry.ImagePath = path;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //Updates the Cache file in the Directory
        public void UpdateCacheFile(List<AnimeEntryObject> input)
        {
            string json = JsonConvert.SerializeObject(input, Newtonsoft.Json.Formatting.Indented);
            try
            {
                string folderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Cache\";
                string filepath = folderpath + @"CachedData.json";

                FileInfo folder = new FileInfo(folderpath);
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
            List<AnimeEntryObject> FileList = new List<AnimeEntryObject>();
            try
            {
                string[] files = Directory.GetFiles(@"C:\Users\littl\Documents\Visual Studio 2015\Projects\Amatsukaze\Amatsukaze\Test Input");                
                foreach (string filename in files)
                {
                    string input = File.ReadAllText(filename);
                    AnimeEntryObject animeentry = new AnimeEntryObject();

                    if (ParseXml(input, animeentry))
                    {
                        animeentry.ContentsDump();
                        FileList.Add(animeentry);
                        DownloadAnimeCover(animeentry);
                    }
                    else
                    {
                        Console.WriteLine("Directory Parse failed");
                        return false;
                    }
                }

                UpdateCacheFile(FileList);
                return true;
            }

            catch (Exception ex)
            {
                //Couldn't fill an animeentryobject correctly
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        
        //Method to parse XML file into an animeentryobject. Throws an exception if some read operation fails.
        //Right now it only reads the first entry... Some name matching might be needed.
        public bool ParseXml(string xmlinput, AnimeEntryObject animeentry)
        {
            try
            {
                string entities = @"<!DOCTYPE documentElement[
                <!ENTITY Alpha ""&#913;"">
                <!ENTITY ndash ""&#8211;"">
                <!ENTITY mdash ""&#8212;"">                
                ]>";

                //Using a regular expression to remove the stupid invalid characters that are present in the XML from MAL
                string input = entities + Regex.Replace(xmlinput, @"&(?!(?:lt|gt|amp|apos|quot|#\d+|#x[a-f\d]+);)", "&amp;", RegexOptions.IgnoreCase);                

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;

                //Create an XML reader
                using (XmlReader reader = XmlReader.Create(new StringReader(input), settings))
                {
                    //Sample input
                    /*
                    <anime>
                        <entry>
                            <id>20785</id>
                            <title>Mahouka Koukou no Rettousei</title>
                            <english>The irregular at magic high school</english>
                            <synonyms/>
                            <episodes>26</episodes>
                            <score>7.80</score>
                            <type>TV</type>
                            <status>Finished Airing</status>
                            <start_date>2014-04-06</start_date>
                            <end_date>2014-09-28</end_date>
                            <synopsis>
                            Magic&mdash;A century has passed since this concept has been recognized as a formal technology instead of the product of the occult or folklore.<br /> <br /> The season is spring and it is time for a brand new school year.<br /> <br /> At the National Magic University First Affiliate High School, a.k.a. Magic High School, students are divided into two distinct groups according to their academic performances. The &quot;Bloom,&quot; who demonstrate the highest grades and are enrolled in the &quot;First Course,&quot; and the &quot;Weed,&quot; who have a poor academic record and are enrolled in the &quot;Second Course.&quot;<br /> <br /> This spring, a very peculiar brother and sister enroll as new students. The brother is an under achiever with some deficiencies and enrolls as a &quot;Weed,&quot; while his younger sister is an honor student, who enrolls as a &quot;Bloom.&quot; The brother, with a somewhat philosophical expression, and the younger sister who holds feelings a little stronger than sibling love for him... Ever since these two have entered through the gates of this prestigious school, the calm campus was beginning to change...<br /> <br /> (Source: Aniplex USA)
                            </synopsis>
                            <image>
                            http://cdn.myanimelist.net/images/anime/11/61039.jpg
                            </image>
                        </entry> 
                    </anime> */

                    //Parsing Operation
                    reader.ReadToFollowing("anime");
                    reader.ReadToDescendant("entry");  //Navigate to entry
                    reader.ReadToDescendant("id");  //Navigate to ID
                    animeentry.id = reader.ReadElementContentAsInt();

                    reader.ReadToFollowing("title"); //Navigate to Title
                    animeentry.title = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("english"); //Navigate to English
                    animeentry.Englishtitle = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("synonyms"); //Navigate to Synonyms
                    animeentry.synonyms = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("episodes"); //Navigate to Episodes
                    animeentry.episodes = reader.ReadElementContentAsInt();

                    reader.ReadToFollowing("score"); //Navigate to Score
                    animeentry.score = reader.ReadElementContentAsDouble();

                    reader.ReadToFollowing("type"); //Navigate to type
                    animeentry.type = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("status"); //Navigate to status
                    animeentry.status = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("start_date");//Navigate to start date
                    animeentry.start_date = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("end_date"); //Navigate to end date
                    animeentry.end_date = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("synopsis"); //Navigate to synopsis
                    animeentry.synopsis = reader.ReadInnerXml();

                    reader.ReadToFollowing("image"); //Navigate to imageURL
                    animeentry.imageURL = reader.ReadElementContentAsString();
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
    }
}
