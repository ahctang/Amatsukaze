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
            SelectedTheme = this.optionsobject.Themesetting;
        }        
       

        #region Fields
        private List<string> availablethemes = new List<string>( new string[] { "Amatsukaze", "Shimakaze" });

        private string selectedtheme;
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

        #endregion



    }
}
