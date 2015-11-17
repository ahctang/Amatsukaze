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
            this.SelectedTheme = this.optionsobject.Themesetting;
            this.CacheFolderPath = optionsobject.CacheFolderpath;
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
