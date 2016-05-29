using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Amatsukaze.HelperClasses
{
    public static class MalXmlParser
    {

        public static void test(string xmlInput)
        {
            try {
                //XDocument doc = XDocument.Load("MALsample.xml");
                XDocument doc = XDocument.Parse(xmlInput);
                List<MalEntry> anime = doc.Root
                           .Elements("entry")
                           .Select(x => new MalEntry
                           {
                               id = (int)x.Element("id"),
                               title = (string)x.Element("title"),
                               english = (string)x.Element("english"),
                               synonyms = (string)x.Element("synonyms"),
                               episodes = (int)x.Element("episodes"),
                               score = (string)x.Element("score"),
                               type = (string)x.Element("type"),
                               status = (string)x.Element("status"),
                               start_date = (string)x.Element("start_date"),
                               end_date = (string)x.Element("end_date"),
                               synopsis = (string)x.Element("synopsis"),
                               image = (string)x.Element("image")
                           })
                           .ToList();
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public class MalEntry
    {
        public int id { get; set; }
        public string title { get; set; }
        public string english { get; set; }
        public string synonyms { get; set; }
        public int episodes { get; set; }
        public string score { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string synopsis { get; set; }
        public string image { get; set; }
    }
}
