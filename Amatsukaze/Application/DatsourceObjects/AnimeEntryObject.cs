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
    public class AnimeEntryObject : ObservableObjectClass, IEquatable<AnimeEntryObject>, IComparable<AnimeEntryObject>
    {
        //Object to hold the properties for one anime (based on the return from myanimelist
        #region Properties

        //Properties used by Amatsukaze
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

        //Staff
        public List<AnimeStaff> Staff { get; set; }

        //Character List    
        public List<AnimeCharacter> Characters { get; set; } 

        //Episode list
        public List<Episode> Episodes { get; set; }

        //Sources used
        public List<string> Sources { get; set; }

        //Properties for Amatsukaze GUI

        //Anime Cover Image
        private string imagePath;
        public string ImagePath
        {
            get
            {
                return imagePath;
            }
            set
            {
                if (imagePath != value)
                {
                    imagePath = value;
                    OnPropertyChanged("ImagePath");
                }
            }
        }

        //For the image grid layout
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

        public AnimeEntryObject()
        {
        }

        public AnimeEntryObject(MALDataSource datasource)
        {
            if (datasource == null)
            {
                return;
            }

            this.Sources = new List<string>();
            this.Sources.Add("MAL");

            PropertyInfo[] properties = datasource.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                PropertyInfo targetproperty = this.GetType().GetProperty(property.Name);
                targetproperty.SetValue(this, property.GetValue(datasource, null), null);
            }
        }

        public AnimeEntryObject(AniDBDataSource datasource)
        {
            if (datasource == null)
            {
                return;
            }

            //Can't use reflection because the names are all different, so have to use a lamo copy constructor
            this.id = datasource.Id;
            this.type = datasource.type;
            this.start_date = datasource.start_date;
            this.end_date = datasource.end_date;
            this.title = datasource.title;
            this.score = datasource.StandardRating;
            this.english = datasource.english;
            this.synonyms = datasource.synonyms;
            this.image = datasource.Picture;
            this.synopsis = datasource.synopsis;
            this.episodes = datasource.Episodes.Count;

            this.Staff = datasource.Staff;
            this.Characters = datasource.Characters;
            this.Episodes = datasource.Episodes;

            AssignGridRowColumn(this.Staff);
            AssignGridRowColumn(this.Characters);

            this.Sources = new List<string>();
            this.Sources.Add("AniDB");
        }

        public AnimeEntryObject(MALDataSource MALdatasource, AniDBDataSource AniDBdatasource)
        {
            if (MALdatasource == null || AniDBdatasource == null)
            {
                return;
            }

            PropertyInfo[] properties = MALdatasource.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                PropertyInfo targetproperty = this.GetType().GetProperty(property.Name);
                targetproperty.SetValue(this, property.GetValue(MALdatasource, null), null);
            }

            this.synopsis = AniDBdatasource.synopsis;
            this.Staff = AniDBdatasource.Staff;
            this.Characters = AniDBdatasource.Characters;
            this.Episodes = AniDBdatasource.Episodes;

            AssignGridRowColumn(this.Staff);
            AssignGridRowColumn(this.Characters);
        }

        //Methods

        public AnimeEntryObject Clone()
        {
            return (AnimeEntryObject)this.MemberwiseClone();
        }

        private void AssignGridRowColumn(List<AnimeCharacter> CharacterList)
        {
            int columncounter = 0, rowcounter = 0;
            foreach (AnimeCharacter Character in CharacterList)
            {
                Character.GridColumn = columncounter;
                Character.GridRow = rowcounter;

                columncounter++;

                if (columncounter > (4 - 1))
                {
                    rowcounter++;
                    columncounter = 0;
                }
            }
        }

        public void MergeAnimeCover(string ImagePath)
        {
            lock(this)
            {
                this.ImagePath = ImagePath;
            }            
        }

        public void MergeCharacters(List<AnimeCharacter> Characters)
        {
            lock(this)
            {
                this.Characters = Characters;
            }

            //Add AniDB just in case because characters can only be added from AniDB
            if (this.Sources.Contains("AniDB") == false) this.Sources.Add("AniDB");
        }

        public void MergeInfo(MALDataSource Input, OptionsObject OptionsObject)
        {
            //Return if OptionsObject doesn't have MALDataSource enabled. 
            if (OptionsObject.UseMALDataSource == false) return;
            
            //Try to merge the property if the property is not defined on AnimeEntryObject
            PropertyInfo[] InputProperties = Input.GetType().GetProperties();

            foreach (PropertyInfo property in InputProperties)
            {
                PropertyInfo AnimeEntryObjectProperty = this.GetType().GetProperty(property.Name);
                if (AnimeEntryObjectProperty != null)
                {
                    if (AnimeEntryObjectProperty.GetValue(this, null) == null && property.GetValue(Input, null) != null)
                    {
                        AnimeEntryObjectProperty.SetValue(this, property.GetValue(Input, null), null);
                    }
                }               
            }

            if (this.Sources.Contains("MAL") == false) this.Sources.Add("MAL");

            //Temporary Workaround
            this.image = Input.image;
            this.synopsis = Input.synopsis;
        }

        public void MergeInfo(AniDBDataSource Input, OptionsObject OptionsObject)
        {
            //Return if OptionsObject doesn't have AniDB enabled. 
            if (OptionsObject.UseAniDBDataSource == false) return;

            //Try to merge the property if the property is not defined on AnimeEntryObject
            PropertyInfo[] InputProperties = Input.GetType().GetProperties();

            foreach (PropertyInfo property in InputProperties)
            {
                PropertyInfo AnimeEntryObjectProperty = this.GetType().GetProperty(property.Name);

                if (AnimeEntryObjectProperty != null)
                {
                    if (AnimeEntryObjectProperty.GetValue(this, null) == null && property.GetValue(Input, null) != null)
                    {
                        AnimeEntryObjectProperty.SetValue(this, property.GetValue(Input, null), null);
                    }
                }                
            }

            if (this.Sources.Contains("AniDB") == false) this.Sources.Add("AniDB");            
        }

        /*public void MergeInfo(AnimeEntryObject Input, OptionsObject OptionsObject)
        {
            //Return if OptionsObject doesn't have AniDB enabled. 
            if (OptionsObject.UseAniDBDataSource == false) return;

            //Try to merge the property if the property is not defined on AnimeEntryObject
            PropertyInfo[] InputProperties = Input.GetType().GetProperties();

            foreach (PropertyInfo property in InputProperties)
            {
                PropertyInfo AnimeEntryObjectProperty = this.GetType().GetProperty(property.Name);

                if (AnimeEntryObjectProperty.GetValue(this, null) == null && property.GetValue(Input, null) != null)
                {
                    AnimeEntryObjectProperty.SetValue(this, property.GetValue(Input, null), null);
                }
            }

            if (this.Sources.Contains("AniDB") == false) this.Sources.Add("AniDB");
        }*/

        private void AssignGridRowColumn(List<AnimeStaff> StaffList)
        {
            int columncounter = 0, rowcounter = 0;
            foreach (AnimeStaff Staff in StaffList)
            {
                Staff.GridColumn = columncounter;
                Staff.GridRow = rowcounter;

                columncounter++;

                if (columncounter > (4 - 1))
                {
                    rowcounter++;
                    columncounter = 0;
                }
            }
        }

        public bool Equals(AnimeEntryObject other)
        {
            //Check if other has an english name
            string otherName;
            string thisName;

            if (other.english.Length != 0 && other.english != null)
            {
                otherName = other.english;
            }
            else
            {
                otherName = other.title;
            }

            //Check if this object has an english name
            if (this.english.Length != 0 && this.english != null)
            {
                thisName = this.english;
            }
            else
            {
                thisName = this.title;
            }

            if (thisName.ToLower().Equals(otherName.ToLower())) return true;
            else return false;
        }

        public int CompareTo(AnimeEntryObject other)
        {
            //Check if other has an english name
            string otherName;
            string thisName;

            if (other.english.Length != 0 && other.english != null)
            {
                otherName = other.english;
            }
            else
            {
                otherName = other.title;
            }

            //Check if this object has an english name
            if (this.english.Length != 0 && this.english != null)
            {
                thisName = this.english;
            }
            else
            {
                thisName = this.title;
            }

            //Do the comparison here
            return thisName.ToLower().CompareTo(otherName.ToLower());
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
