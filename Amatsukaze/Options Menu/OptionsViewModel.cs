using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;
using Amatsukaze.Model;
using System.Windows;


namespace Amatsukaze.ViewModel
{
    class OptionMenuViewModel : ObservableObjectClass, ViewModelBase
    {
        public string BaseName
        {
            get
            {
                return "Options Menu";
            }
        }


        public OptionMenuViewModel(OptionsObject optionsobject, IEventAggregator eventAggregator)
        {
            this.optionsobject = optionsobject;
            this.SelectedTheme = optionsobject.Themesetting;
            this.CacheFolderPath = optionsobject.CacheFolderpath;
            this.UseMALDataSource = optionsobject.UseMALDataSource;
            this.UseAniDBDataSource = optionsobject.UseAniDBDataSource;
            this.AniDBImageURL = optionsobject.AniDBImageURL;
            this.AniDBXMLQueryURL = optionsobject.AniDBXMLQueryURL;
            this.AniDBRetrieverURL = optionsobject.AniDBIDRetrieverURL;
            this.MALXMLQueryURL = optionsobject.MALXMLQueryURL;
            
            this.EventAggregator = eventAggregator;
        }

        #region Objects
        OptionsModel optionsmodel = new OptionsModel();
        OptionsObject optionsobject;
        public IEventAggregator EventAggregator;
        #endregion                

        #region Fields
        private List<string> availablethemes = new List<string>( new string[] { "Amatsukaze", "Shimakaze" });

        private string selectedtheme;
        private string cachefolderpath;
        private bool useMALDataSource;
        private bool useAniDBDataSource;
        private string aniDBImageURL;
        private string aniDBXMLQueryURL;
        private string aniDBRetrieverURL;
        private string malXMLQueryURL;
        private string malLogin;
        private string malPassword;
        #endregion

        #region Properties
        public List<string> AvailableThemes
        {
            get
            {
                return availablethemes;
            }
        }

        public string SelectedTheme
        {
            get
            {                
                return selectedtheme;
            }
            set
            {
                if (selectedtheme != value)
                {
                    selectedtheme = value;
                    optionsobject.Themesetting = value;
                    ApplyTheme(selectedtheme);

                    optionsobject.Save();
                    OnPropertyChanged("SelectedTheme");                                        
                }
            }
        }

        public string CacheFolderPath
        {
            get
            {
                return cachefolderpath;
            }
            set
            {
                if (cachefolderpath != value)
                {
                    cachefolderpath = value;
                    optionsobject.CacheFolderpath = value;

                    optionsobject.Save();
                    OnPropertyChanged("CacheFolderPath");
                }
            }
        }

        public bool UseMALDataSource
        {
            get
            {
                return useMALDataSource;
            }
            set
            {
                if (useMALDataSource != value)
                {
                    useMALDataSource = value;
                    optionsobject.UseMALDataSource = value;

                    optionsobject.Save();
                    OnPropertyChanged("UseMALDataSource");
                }
            }
        }

        public bool UseAniDBDataSource
        {
            get
            {
                return useAniDBDataSource;
            }
            set
            {
                if (useAniDBDataSource != value)
                {
                    useAniDBDataSource = value;
                    optionsobject.UseAniDBDataSource = value;

                    optionsobject.Save();
                    OnPropertyChanged("UseAniDBDataSource");
                }
            }
        }

        public string AniDBImageURL
        {
            get
            {
                return aniDBImageURL;
            }
            set
            {
                if (aniDBImageURL != value)
                {
                    aniDBImageURL = value;
                    optionsobject.AniDBImageURL = value;

                    optionsobject.Save();
                    OnPropertyChanged("AniDBImageURL");
                }
            }
        }

        public string AniDBXMLQueryURL
        {
            get
            {
                return aniDBXMLQueryURL;
            }
            set
            {
                if (aniDBXMLQueryURL != value)
                {
                    aniDBXMLQueryURL = value;
                    optionsobject.AniDBXMLQueryURL = value;

                    optionsobject.Save();
                    OnPropertyChanged("AniDBXMLQueryURL");
                }
            }
        }

        public string AniDBRetrieverURL
        {
            get
            {
                return aniDBRetrieverURL;
            }
            set
            {
                if (aniDBRetrieverURL != value)
                {
                    aniDBRetrieverURL = value;
                    optionsobject.AniDBIDRetrieverURL = value;

                    optionsobject.Save();
                    OnPropertyChanged("AniDBRetrieverURL");
                }
            }
        }

        public string MALXMLQueryURL
        {
            get
            {
                return malXMLQueryURL;
            }
            set
            {
                if (malXMLQueryURL != value)
                {
                    malXMLQueryURL = value;
                    optionsobject.MALXMLQueryURL = value;

                    optionsobject.Save();
                    OnPropertyChanged("MALXMLQueryURL");
                }
            }
        }

        public string MALLogin
        {
            get
            {
                return malLogin;
            }
            set
            {
                if (malLogin != value)
                {
                    malLogin = value;
                    optionsobject.MALLogin = value;

                    optionsobject.Save();
                    OnPropertyChanged("MALLogin");
                }
            }
        }

        public string MALPassword
        {
            get
            {
                return malPassword;
            }
            set
            {
                if (malPassword != value)
                {
                    malPassword = value;
                    optionsobject.MALPassword = value;

                    optionsobject.Save();
                    OnPropertyChanged("MALPassword");
                }
            }
        }
        #endregion

        #region Methods

        private void ApplyTheme(string theme)
        {
            string input = @"/Resources/" + optionsobject.Themesetting + ".xaml";
            Uri uri1;

            if (Uri.TryCreate(input, UriKind.Relative, out uri1))
            {
                var app = Application.Current as App;
                app.ChangeTheme(uri1);
                SendMessagetoGUI("Options: " + theme + " theme applied.");
                
            }
            else
            {
                SendMessagetoGUI("Options: " + theme + " theme not found.");
                return;
            }            
        }

        private void SendMessagetoGUI(string message)
        {
            if (this.EventAggregator != null)
            {
                this.EventAggregator.PublishEvent(new MessagetoGUI() { Message = message });
            }
        }

        #endregion

        #region Events/Handlers        

        
        #endregion

    }
}
