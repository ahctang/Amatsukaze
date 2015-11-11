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

namespace Amatsukaze.Model
{
    class LibraryMenuModel
    {        
        public LibraryMenuModel (OptionsObject optionsobject)
        {
            this.optionsobject = optionsobject;
        }

        #region #objects
        public List<AnimeEntryObject> AnimeLibraryList = new List<AnimeEntryObject>();
        OptionsObject optionsobject;

        #endregion


        #region methods

        //Reads the cache file, but returns nothing if the cache hasn't been created yet
        public void ReadCacheFile()
        {            
            string filepath = optionsobject.CacheFolderpath + @"\CachedData.json";
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
            else
            {
                FileInfo folderpath = new FileInfo(optionsobject.CacheFolderpath);
                folderpath.Directory.Create();
            }
        }

        //Downloads the image file using the image in the animeentryobject
        public void DownloadAnimeCover(AnimeEntryObject animeentry)
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        //Updates the Cache file in the Directory (appends the input list to the existing json file). WILL RESULT IN DUPLICATES IF NOT CAREFUL
        public void SaveCacheFile(List<AnimeEntryObject> input)
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
            List<AnimeEntryObject> FileList = new List<AnimeEntryObject>();
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
                        AnimeEntryObject animeentry = new AnimeEntryObject();

                        if (ParseXml(input, animeentry))
                        {
                            animeentry.ContentsDump();
                            FileList.Add(animeentry);
                            DownloadAnimeCover(animeentry);
                            File.Delete(filename);
                        }
                        else
                        {
                            Console.WriteLine("Directory Parse failed");
                            return false;
                        }
                    }

                    AnimeLibraryList.AddRange(FileList);
                    SaveCacheFile(AnimeLibraryList);
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

                    //Loops until it finds the /entry element
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        reader.Read();

                        //Note: Because of the reflection used, every single property in the animeentry object must have the same name as the property in the xml 
                        PropertyInfo property = animeentry.GetType().GetProperty(reader.Name);
                        if (property != null)
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                //only synopsis needs ReadInnerXML because it contains illegal HTML stuff thanks to MAL
                                if (property.Name == "synopsis")
                                {
                                    property.SetValue(animeentry, reader.ReadInnerXml(), null);
                                }
                                else
                                {
                                    property.SetValue(animeentry, reader.ReadElementContentAsString(), null);
                                }
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                property.SetValue(animeentry, reader.ReadElementContentAsInt(), null);
                            }
                            else if (property.PropertyType == typeof(double))
                            {
                                property.SetValue(animeentry, reader.ReadElementContentAsDouble(), null);
                            }
                        }
                    }
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
