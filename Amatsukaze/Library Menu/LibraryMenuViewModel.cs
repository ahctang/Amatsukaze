using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;
using Amatsukaze.Model;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;

namespace Amatsukaze.ViewModel
{
    class LibraryMenuViewModel : ObservableObjectClass, ViewModelBase
    {
        public string BaseName
        {
            get
            {
                return "Library Menu";
            }
        }

        public LibraryMenuViewModel(OptionsObject optionsobject)
        {
            this.optionsobject = optionsobject;

            LibraryMenuModel datasource = new LibraryMenuModel(optionsobject);

            datasource.ReadCacheFile();
            datasource.ReadXMLDirectory();    //Running this method here is probably not right....        

            animelibrarylist = new ObservableCollection<AnimeEntryObject>(datasource.AnimeLibraryList);                               
                             
        }

        #region Objects

        OptionsObject optionsobject;
        private ObservableCollection<AnimeEntryObject> animelibrarylist;
        #endregion

        #region Properties
        public ObservableCollection<AnimeEntryObject> AnimeLibraryList
        {
            get
            {
                return animelibrarylist;
            }
            set
            {
                if (animelibrarylist != value)
                {
                    animelibrarylist = value;
                    OnPropertyChanged("AnimeLibraryList");
                }
            }

        }
        #endregion

        #region Methods

        public void UpdateGridIndexes (int ColumnCount)
        {
            int columncounter = 0, rowcounter = 0;
            foreach (AnimeEntryObject anime in this.AnimeLibraryList)
            {
                anime.GridColumn = columncounter;
                anime.GridRow = rowcounter;

                columncounter++;                

                if (columncounter > (ColumnCount - 1))
                {
                    rowcounter++;
                    columncounter = 0;
                }
            }
        }

        #endregion

        #region EventHandlers
        public void DisplayAreaResized(int columncount)
        {
            if (columncount != 0)
            {
                UpdateGridIndexes(columncount);
                GridColumnCount = columncount;
                if (AnimeLibraryList.Count % columncount == 0)
                {
                    GridRowCount = AnimeLibraryList.Count / columncount;
                }

                else
                {
                    GridRowCount = (AnimeLibraryList.Count / columncount) + 1;
                }
            }                        
        }

        #endregion

        #region Fields

        private int gridcolumncount;
        private int gridrowcount;
        #endregion

        #region Properties

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
        #endregion

    }
}
