using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;

namespace Amatsukaze.ViewModel
{
    public class SortedDisplayInfoHolder : ObservableObjectClass
    {
        public string SortCriteria { get; set; }  
        private ObservableCollection<AnimeEntryObject> animeEntries = new ObservableCollection<AnimeEntryObject>();
        public ObservableCollection<AnimeEntryObject> AnimeEntries
        {
            get
            {
                return animeEntries;
            }            
        }

        private int gridcolumncount;
        public int GridColumnCount
        {
            get
            {
                return gridcolumncount;
            }
            set
            {
                if (gridcolumncount != value)
                {
                    gridcolumncount = value;
                    OnPropertyChanged("GridColumnCount");
                }
            }
        }

        private int gridrowcount { get; set; }
        public int GridRowCount
        {
            get
            {
                return gridrowcount;
            }
            set
            {
                if (gridrowcount != value)
                {
                    gridrowcount = value;
                    OnPropertyChanged("GridRowCount");
                }
            }
        }
    }
}
