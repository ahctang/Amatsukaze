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

    public class AniDBDataSource : XMLDataSource
    {
        //This function will parse the fields I'm planning to take from an AniDB XML entry. It doesn't take everything, so it maybe worth redoing in the future.
        //Standard fields
        public int Id { get; set; }
        public string Type { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Title { get; set; }
        public string EnglishTitle { get; set; }
        public string Synonyms { get; set; }

        //Skipping related anime/similar anime nodes/recommendations

        //Staff
        public List<AnimeStaff> Staff;

        //Synoposis
        public string Synopsis { get; set; }


        //Ratings
        public double WeightedRating { get; set; }
        public double StandardRating { get; set; }

        //Picture URL
        public string Picture { get; set; }

        //Skipping resources/tags        

        //Character List    
        public List<AnimeCharacter> Characters;

        //Episode list
        public List<Episode> Episodes;

        //Debug dump
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

    public class AnimeStaff
    {
        public string Position { get; set; }
        public string Name { get; set; }
    }

    public class AnimeCharacter
    {
        public string CharacterName { get; set; }
        public string CharacterPicture { get; set; }
        public string Seiyuu { get; set; }
        public string Picture { get; set; }
    }

    public class Episode
    {
        public int Epno { get; set; }
        public string Title { get; set; }
    }
}

