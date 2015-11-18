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
