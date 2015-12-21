using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Amatsukaze.ViewModel
{
    public static class XMLParsers
    {
        //Method to parse XML file into an datasourceobject. Throws an exception if some read operation fails.
        //Right now it only reads the first entry... Some name matching might be needed.
        public static bool MALParseXML(string xmlinput, MALDataSource _MALDataSource)
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
                        PropertyInfo property = _MALDataSource.GetType().GetProperty(reader.Name);
                        if (property != null)
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                //only synopsis needs ReadInnerXML because it contains illegal HTML stuff thanks to MAL
                                if (property.Name == "synopsis")
                                {
                                    property.SetValue(_MALDataSource, reader.ReadInnerXml(), null);
                                }
                                else
                                {
                                    property.SetValue(_MALDataSource, reader.ReadElementContentAsString(), null);
                                }
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                property.SetValue(_MALDataSource, reader.ReadElementContentAsInt(), null);
                            }
                            else if (property.PropertyType == typeof(double))
                            {
                                property.SetValue(_MALDataSource, reader.ReadElementContentAsDouble(), null);
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

        //Method to parse XML file into an datasourceobject. Throws an exception if some read operation fails.        
        public static bool AniDBParseXML(string xmlinput, AniDBDataSource aniDBDataSource)
        {
            //Before beginning we need to strip out the illegal characters (sigh)
            xmlinput = Regex.Replace(xmlinput, @"&(?!(?:lt|gt|amp|apos|quot|#\d+|#x[a-f\d]+);)", "&amp;", RegexOptions.IgnoreCase);

            try
            {
                //Create an XML reader
                using (XmlReader reader = XmlReader.Create(new StringReader(xmlinput)))
                {
                    //Doing this super manually because of the complexity of the aniDB xml. 
                    reader.ReadToFollowing("anime");
                    aniDBDataSource.Id = Convert.ToInt16(reader.GetAttribute("id"));

                    reader.ReadToFollowing("type");
                    aniDBDataSource.Type = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("startdate");
                    aniDBDataSource.StartDate = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("enddate");
                    aniDBDataSource.EndDate = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("titles");
                    reader.ReadToDescendant("title");
                    do
                    {
                        if (reader.GetAttribute("xml:lang") == "x-jat" && reader.GetAttribute("type") == "main")
                        {
                            aniDBDataSource.Title = reader.ReadElementContentAsString();
                        }
                        else if (reader.GetAttribute("xml:lang") == "x-jat" && reader.GetAttribute("type") == "synonym")
                        {
                            aniDBDataSource.Synonyms = reader.ReadElementContentAsString();
                        }
                        else if (reader.GetAttribute("xml:lang") == "en")
                        {
                            aniDBDataSource.EnglishTitle = reader.ReadElementContentAsString();
                        }

                    } while (reader.ReadToNextSibling("title"));


                    reader.ReadToFollowing("creators");
                    reader.ReadToDescendant("name");
                    do
                    {
                        AnimeStaff staffmember = new AnimeStaff();

                        staffmember.Position = reader.GetAttribute("type");
                        staffmember.Name = reader.ReadElementContentAsString();

                        if (aniDBDataSource.Staff == null) aniDBDataSource.Staff = new List<AnimeStaff>();
                        aniDBDataSource.Staff.Add(staffmember);
                    } while (reader.ReadToNextSibling("name"));

                    reader.ReadToFollowing("description");
                    aniDBDataSource.Synopsis = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("permanent");
                    aniDBDataSource.WeightedRating = Convert.ToDouble(reader.ReadElementContentAsString());

                    reader.ReadToFollowing("temporary");
                    aniDBDataSource.StandardRating = Convert.ToDouble(reader.ReadElementContentAsString());

                    reader.ReadToFollowing("picture");
                    aniDBDataSource.Picture = reader.ReadElementContentAsString();

                    reader.ReadToFollowing("characters");
                    reader.ReadToDescendant("character");

                    int charactercounter = 0;
                    do
                    {
                        AnimeCharacter animecharacter = new AnimeCharacter();

                        reader.ReadToDescendant("name");
                        animecharacter.CharacterName = reader.ReadElementContentAsString();

                        reader.ReadToNextSibling("picture");
                        if (reader.NodeType != XmlNodeType.EndElement) animecharacter.CharacterPicture = reader.ReadElementContentAsString();
                        else goto endofcycle; //Skip to the end if the entry is malformed (to stop the parsing process from exploding)

                        reader.ReadToNextSibling("seiyuu");
                        if (reader.NodeType != XmlNodeType.EndElement)
                        {
                            animecharacter.Picture = reader.GetAttribute("picture");
                            animecharacter.Seiyuu = reader.ReadElementContentAsString();
                        }
                        else goto endofcycle;

                        endofcycle:
                        if (aniDBDataSource.Characters == null) aniDBDataSource.Characters = new List<AnimeCharacter>();
                        aniDBDataSource.Characters.Add(animecharacter);

                        //We're only going to take an arbitrary 8 characters
                        charactercounter++;

                        //Push the reader to the closing element (/character) so ReadtoNextsibling doens't fail
                        if (reader.NodeType != XmlNodeType.EndElement) reader.ReadEndElement();
                    } while (reader.ReadToNextSibling("character") && charactercounter < 8);

                    reader.ReadToFollowing("episodes");
                    reader.ReadToDescendant("episode");

                    do
                    {
                        Episode episode = new Episode();

                        reader.ReadToDescendant("epno");
                        string type = reader.GetAttribute("type");
                        string epbuffer = reader.ReadElementContentAsString();

                        while (reader.ReadToNextSibling("title"))
                        {
                            if ((reader.GetAttribute("xml:lang") == "en") && (type == "1"))
                            {
                                episode.Epno = Convert.ToInt16(epbuffer);
                                episode.Title = reader.ReadElementContentAsString();
                                if (aniDBDataSource.Episodes == null) aniDBDataSource.Episodes = new List<Episode>();
                                aniDBDataSource.Episodes.Add(episode);
                            }
                        }

                    } while (reader.ReadToNextSibling("episode"));
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
    }
}

