using System;
using System.Collections.Generic;
using System.Text;
using Amatsukaze.HelperClasses;
using Amatsukaze.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Windows;
using System.Linq;

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

        public LibraryMenuViewModel(OptionsObject optionsobject, IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            this.optionsobject = optionsobject;
            datasource = new LibraryMenuModel(optionsobject);

            //Subscribe to events
            datasource.SendMessagetoGUI += new EventHandler(onSendMessagetoGUI);
            this.animeLibraryList.CollectionChanged += OnAnimeLibraryListCollectionChanged;
            this.SeasonSortList.CollectionChanged += OnSeasonSortListCollectionChanged;            

            //Read the cache file
            datasource.ReadCacheFile();

            //Initialize animeLibraryList (for view all)
            this.animeLibraryList = datasource.AnimeLibraryList;

            //Season sort           
            this.InitializeSeasonLists();            
        }


        #region Fields
        
        //Commands
        private ICommand _refreshCommand;
        private ICommand _selectAnime;
        private ICommand _switchSort;

        //Private fields to keep track of the current grid column and row count
        private int gridcolumncount;
        private int gridrowcount;

        //Private field for the currently displayed anime
        private AnimeEntryObject selectedAnime;

        //Private field for the current sort view
        private string currentView = "All";

        //GUI Toggles
        private bool messageLogToggle;
        private bool animeInfoToggle;
        #endregion    

        #region Objects

        OptionsObject optionsobject;
        LibraryMenuModel datasource;

        //Private copy of the message log displayed in the drop down menu. Added to in the SendMessagetoGUI event handler
        private ObservableCollection<string> libraryMessageLog = new ObservableCollection<string>();
        private ObservableCollection<AnimeEntryObject> animeLibraryList = new ObservableCollection<AnimeEntryObject>();
        private ObservableCollection<SortedDisplayInfoHolder> seasonSortList = new ObservableCollection<SortedDisplayInfoHolder>();

        public IEventAggregator EventAggregator { get; set; }        

        #endregion

        #region Properties/Commands

        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand(
                        p => Refresh(),
                        p => true);
                }
                return _refreshCommand;
            }
        }

        public ICommand SelectAnime
        {
            get
            {
                if (_selectAnime == null)
                {
                    _selectAnime = new RelayCommand(
                        p => Select((AnimeEntryObject)p),
                        p => true);
                }
                return _selectAnime;
            }
        }

        public ICommand SwitchSort
        {
            get
            {
                if (_switchSort == null)
                {
                    _switchSort = new RelayCommand(
                        p => Switch((string)p),
                        p => true);
                }
                return _switchSort;
            }
        }

        public ObservableCollection<AnimeEntryObject> AnimeLibraryList
        {
            get
            {
                return animeLibraryList;
            }
        }

        public ObservableCollection<SortedDisplayInfoHolder> SeasonSortList
        {
            get
            {
                return seasonSortList;
            }
        }
        
        public ObservableCollection<string> LibraryMessageLog
        {
            get
            {
                return libraryMessageLog;
            }
            set
            {
                if (libraryMessageLog != value)
                {
                    libraryMessageLog = value;
                    OnPropertyChanged("LibraryMessageLog");
                }
            }
        }

        //Display Grid Counts for Default Library View
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

        //Display Grid Counts for Default Library View
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

        public bool MessageLogToggle
        {
            get
            {
                return messageLogToggle;
            }
            set
            {
                if (messageLogToggle != value)
                {
                    messageLogToggle = value;
                    OnPropertyChanged("MessageLogToggle");
                }
            }
        }

        public bool AnimeInfoToggle
        {
            get
            {
                return animeInfoToggle;
            }
            set
            {
                if (animeInfoToggle != value)
                {
                    animeInfoToggle = value;
                    OnPropertyChanged("AnimeInfoToggle");
                }
            }
        }

        public string CurrentView
        {
            get
            {
                return currentView;
            }
            set
            {
                if (currentView != value)
                {
                    currentView = value;
                    OnPropertyChanged("CurrentView");
                }
            }
        }
    

        public AnimeEntryObject SelectedAnime
        {
            get
            {
                return selectedAnime;
            }
            set
            {
                if (selectedAnime != value)
                {
                    selectedAnime = value;
                    OnPropertyChanged("SelectedAnime");
                }
            }
        }

        #endregion

        #region Methods

        //Updates
        private void UpdateGridIndexes(int ColumnCount, ObservableCollection<AnimeEntryObject> AnimeList)
        {
            int columncounter = 0, rowcounter = 0;
            foreach (AnimeEntryObject anime in AnimeList)
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

        //Rescans the Cache folder for XML files
        private async void Refresh()
        {
            await datasource.ReadXMLDirectoryAsync();
        }

        //Changes the currently selected anime
        private void Select(AnimeEntryObject anime)
        {
            this.SelectedAnime = anime;
            this.AnimeInfoToggle = true;
        }

        //Switches the current sort view (the buttons on the top)
        private void Switch(string targetView)
        {
            if (CurrentView != targetView)
                CurrentView = targetView;
            Console.WriteLine("Current View: {0}", CurrentView);
            return;
        }

        private void InitializeSeasonLists()
        {
            List<SortedDisplayInfoHolder> temporarylist = new List<SortedDisplayInfoHolder>();
            //Go through all of the anime to dig out which dates are present and add them to their respective collections
            foreach (AnimeEntryObject anime in this.AnimeLibraryList)
            {
                
                int SeasonSortCollectionIndex = temporarylist.FindIndex(season => season.SortCriteria == SeasonConverter(anime.start_date));

                //If the index is found it won't be -1
                if (SeasonSortCollectionIndex != -1)
                {
                    temporarylist[SeasonSortCollectionIndex].AnimeEntries.Add(anime.Clone());
                }
                else
                {
                    //Make a new one if the season isn't found
                    SortedDisplayInfoHolder buffer = new SortedDisplayInfoHolder();
                    buffer.SortCriteria = SeasonConverter(anime.start_date);                    
                    buffer.AnimeEntries.Add(anime.Clone());

                    temporarylist.Add(buffer);
                }
            }

            //Sort the List
           temporarylist.Sort(CompareAnimeSeasons);

            //Turn the list into the observable collection for binding
            seasonSortList = new ObservableCollection<SortedDisplayInfoHolder>(temporarylist);

            return;
        }

        private string SeasonConverter(string startDate)
        {
            string ret;
            //Check the format of the date
            try
            {
                if (startDate.Length != 10) throw new Exception();
            }
            catch
            {
                MessageBox.Show("Cache file corrupted");
                return "None";
            }

            //Take the year
            string year = startDate.Substring(0, 4);

            //Convert the month to a season
            string season = "";
            int month = Convert.ToInt16(startDate.Substring(5, 2));

            if (month <= 3) season = "Winter";
            else if (month > 3 && month <= 6) season = "Spring";
            else if (month > 6 && month <= 9) season = "Summer";
            else if (month > 9 && month <= 12) season = "Fall";

            //Return a combination of season and year
            ret = season + " " + year;
            return ret;
        }

        //Delegate for comparing shit.
        private static int CompareAnimeSeasons(SortedDisplayInfoHolder x, SortedDisplayInfoHolder y)
        {            
            int xYear = Convert.ToInt16(x.SortCriteria.Substring(x.SortCriteria.IndexOf(" ") + 1));
            int yYear = Convert.ToInt16(y.SortCriteria.Substring(y.SortCriteria.IndexOf(" ") + 1));
            string xMonth = x.SortCriteria.Substring(0, x.SortCriteria.IndexOf(" "));
            string yMonth = y.SortCriteria.Substring(0, y.SortCriteria.IndexOf(" "));

            if (xYear > yYear)
            {
                return 1;
            }
            else if (xYear < yYear)
            {
                return -1;
            }
            else
            {
                if (xMonth == "Winter")
                {
                    if (yMonth == "Winter")
                        return 0;
                    else if (yMonth == "Spring")
                        return -1;
                    else if (yMonth == "Summer")
                        return -1;
                    else if (yMonth == "Fall")
                        return -1;
                }
                else if (xMonth == "Spring")
                {
                    if (yMonth == "Winter")
                        return 1;
                    else if (yMonth == "Spring")
                        return 0;
                    else if (yMonth == "Summer")
                        return -1;
                    else if (yMonth == "Fall")
                        return -1;
                }
                else if (xMonth == "Summer")
                {
                    if (yMonth == "Winter")
                        return 1;
                    else if (yMonth == "Spring")
                        return 1;
                    else if (yMonth == "Summer")
                        return 0;
                    else if (yMonth == "Fall")
                        return -1;
                }
                else if (xMonth == "Fall")
                {
                    if (yMonth == "Winter")
                        return 1;
                    else if (yMonth == "Spring")
                        return 1;
                    else if (yMonth == "Summer")
                        return 1;
                    else if (yMonth == "Fall")
                        return 0;
                }
            }
            return 0;                                
        }

        #endregion

        #region Events/EventHandlers

        //All SendtoGUI events from the model are handled here. and they send the message to the EventAggregator which calls up the black popup menu 
        private void onSendMessagetoGUI(object sender, EventArgs e)
        {
            Console.WriteLine("Library Event Fired!");
            var message = (e as MessageArgs).Message;
            LibraryMessageLog.Add(message);

            if (this.EventAggregator != null)
            {
                this.EventAggregator.PublishEvent(new MessagetoGUI() { Message = message });
            }
        }

        //Every time the size of the display area is changed in the view, the view is hardcoded to call this function to 
        //recalculate the number of columns/rows that fit and to reassign the grid index of every single picture.
        //THIS IS FOR THE FULL LIBRARY VIEW ONLY
        public void LibraryViewAreaResized(int columncount)
        {
            if (columncount != 0)
            {
                UpdateGridIndexes(columncount, this.AnimeLibraryList);
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

        public void SeasonSortListAreaResized(int columncount)
        {
            if (columncount != 0)
            {
                for (int i = 0; i < SeasonSortList.Count; i++)
                {
                    UpdateGridIndexes(columncount, this.SeasonSortList[i].AnimeEntries);
                    SeasonSortList[i].GridColumnCount = columncount;
                    if (SeasonSortList[i].AnimeEntries.Count % columncount == 0)
                    {
                        SeasonSortList[i].GridRowCount = SeasonSortList[i].AnimeEntries.Count / columncount;
                    }

                    else
                    {
                        SeasonSortList[i].GridRowCount = (SeasonSortList[i].AnimeEntries.Count / columncount) + 1;
                    }
                }                
            }
        }

        private void OnAnimeLibraryListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            LibraryViewAreaResized(this.GridColumnCount);
        }

        private void OnSeasonSortListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SeasonSortListAreaResized(this.GridColumnCount);
        }

        #endregion


    }
}
