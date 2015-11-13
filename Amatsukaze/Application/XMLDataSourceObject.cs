using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using Amatsukaze.HelperClasses;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace Amatsukaze.ViewModel
{
    public interface XMLDataSource
    {
        //Method to parse XML file into an datasourceobject. Throws an exception if some read operation fails.
        //Right now it only reads the first entry... Some name matching might be needed.
        bool ParseXML(string xmlinput);
        
        //Dumps the object to the console
        void ContentsDump();
    }

    public class MALDataSource : XMLDataSource
    {
        //Properties from MyAnimeList
        public int id { get; set; }
        public string title { get; set; }
        public string english { get; set; }
        public string synonyms { get; set; }
        public int episodes { get; set; }
        public double score { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string synopsis { get; set; }
        public string image { get; set; }

        //Method to parse XML file into an datasourceobject. Throws an exception if some read operation fails.
        //Right now it only reads the first entry... Some name matching might be needed.
        public bool ParseXML(string xmlinput)
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
                        PropertyInfo property = this.GetType().GetProperty(reader.Name);
                        if (property != null)
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                //only synopsis needs ReadInnerXML because it contains illegal HTML stuff thanks to MAL
                                if (property.Name == "synopsis")
                                {
                                    property.SetValue(this, reader.ReadInnerXml(), null);
                                }
                                else
                                {
                                    property.SetValue(this, reader.ReadElementContentAsString(), null);
                                }
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                property.SetValue(this, reader.ReadElementContentAsInt(), null);
                            }
                            else if (property.PropertyType == typeof(double))
                            {
                                property.SetValue(this, reader.ReadElementContentAsDouble(), null);
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
        
        public void ContentsDump()
        {
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                Console.WriteLine("{0}: {1}", property.Name, property.GetValue(this, null));
            }
        }
    }


    public class AnimeEntryObject : ObservableObjectClass
    {
        //Object to hold the properties for one anime (based on the return from myanimelist
        #region Properties

        //Properties used by Amatsukaze
        public int id { get; set; }
        public string title { get; set; }
        public string english { get; set; }
        public string synonyms { get; set; }
        public int episodes { get; set; }
        public double score { get;set; }
        public string type { get; set; }
        public string status { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string synopsis { get; set; }
        public string image { get; set; }

        //Properties for Amatsukaze GUI
        public string ImagePath { get; set; }

        private int gridcolumn;
        public int GridColumn
        {
            get
            {
                return gridcolumn;
            }
            set
            {
                if (gridcolumn != value)
                {
                    gridcolumn = value;
                    OnPropertyChanged("GridColumn");
                }
            }
        }      

        private int gridrow { get; set; }
        public int GridRow
        {
            get
            {
                return gridrow;
            }
            set
            {
                if (gridrow != value)
                {
                    gridrow = value;
                    OnPropertyChanged("GridRow");
                }
            }
        }

        //Constructors

        public AnimeEntryObject(MALDataSource datasource)
        {
            if (datasource == null)
            {
                return;
            }

            PropertyInfo[] properties = datasource.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                PropertyInfo targetproperty = this.GetType().GetProperty(property.Name);
                targetproperty.SetValue(this, property.GetValue(datasource, null), null);
            }
        }


        [Conditional("DEBUG")]
        public void ContentsDump()
        {
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                Console.WriteLine("{0}: {1}", property.Name, property.GetValue(this, null));
            }
        }
        #endregion
    }
}
