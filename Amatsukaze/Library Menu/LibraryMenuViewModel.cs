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
using System.Windows.Input;
using System.Timers;

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
            datasource = new LibraryMenuModel(optionsobject);

            //Subscribe to events
            datasource.SendMessagetoGUI += new EventHandler(onSendMessagetoGUI);

            datasource.ReadCacheFile();            

            animeLibraryList = datasource.AnimeLibraryList;            
        }

        #region Objects

        OptionsObject optionsobject;
        private ObservableCollection<AnimeEntryObject> animeLibraryList;
        LibraryMenuModel datasource;

        #endregion

        #region Properties/Commands

        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand(
                        p => refresh(),
                        p => true);
                }
                return _refreshCommand;
            }
        }

        public ObservableCollection<AnimeEntryObject> AnimeLibraryList
        {
            get
            {
                return animeLibraryList;
            }
            set
            {
                if (animeLibraryList != value)
                {
                    animeLibraryList = value;
                    OnPropertyChanged("AnimeLibraryList");                    
                }
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

        public string StatusText
        {
            get
            {
                return statustext;
            }
            set
            {
                if (statustext != value)
                {
                    statustext = value;
                    OnPropertyChanged("StatusText");
                }
            }
        }

        public bool MessageTextToggle
        {
            get
            {
                return messageTextToggle;
            }
            set
            {
                if (messageTextToggle != value)
                {
                    messageTextToggle = value;
                    OnPropertyChanged("MessageTextToggle");
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

        private void refresh()
        {
            datasource.ReadXMLDirectory();
            DisplayAreaResized(this.GridColumnCount);
        }

        #endregion

        #region Events/EventHandlers
        void onSendMessagetoGUI (object sender, EventArgs e)
        {
            Console.WriteLine("Event Fired!");
            //Reset the message timer
            t.Stop();
            t.Start();

            var args = e as MessageArgs;
            string message = args.Message;
            StatusText = message;
            LibraryMessageLog.Add(message);

            //Show the message panel
            MessageTextToggle = true;

            //Go to event handler that closes the message panel
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

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

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Stop();
            MessageTextToggle = false;
        }

        #endregion

        #region Fields

        private Timer t = new Timer(3000);
        private ObservableCollection<string> libraryMessageLog = new ObservableCollection<string>();

        private ICommand _refreshCommand;

        private int gridcolumncount;
        private int gridrowcount;

        private string statustext;
        private bool messageTextToggle;
        private bool messageLogToggle;
        #endregion       

    }
}
