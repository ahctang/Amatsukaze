using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace Amatsukaze.ViewModel
{
    class AnimeEntryObject
    {
        //Object to hold the properties for one anime (based on the return from myanimelist
        #region Properties

        //Properties from MyAnimeList
        public int id { get; set; }
        public string title { get; set; }
        public string Englishtitle { get; set; }
        public string synonyms { get; set; }
        public int episodes { get; set; }
        public double score { get;set; }
        public string type { get; set; }
        public string status { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string synopsis { get; set; }
        public string imageURL { get; set; }

        //Properties for Amatsukaze
        public string ImagePath { get; set; }
        public int gridcolumn { get; set; }
        public int gridrow { get; set; }

        [Conditional("DEBUG")]
        public void ContentsDump()
        {
            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                Console.WriteLine("{0}; {1}", property.Name, property.GetValue(this, null));
            }
        }





        #endregion
    }
}
