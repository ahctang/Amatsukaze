using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;
using Amatsukaze.Model;


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

        OptionsModel optionsmodel = new OptionsModel();
        OptionsObject optionsobject;

        public OptionMenuViewModel(OptionsObject optionsobject)
        {
            this.optionsobject = optionsobject;
            this.SelectedTheme = this.optionsobject.Themesetting;
            this.CacheFolderPath = optionsobject.CacheFolderpath;
        }        
       

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

                    optionsmodel.SaveOptionsFile(optionsobject);
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

                    optionsmodel.SaveOptionsFile(optionsobject);
                    OnPropertyChanged("CacheFolderPath");
                }
            }
        }

        #endregion



    }
}
