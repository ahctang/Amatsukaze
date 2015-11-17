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
    public interface XMLDataSource
    {
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
}

