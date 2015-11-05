using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;

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

        #region Fields
        private string selectedtheme;
        #endregion

        #region Properties

        public string SelectedTheme
        {
            get
            {
                if (selectedtheme == null) selectedtheme = "Amatsukaze";                
                return selectedtheme;
            }
            set
            {
                if (selectedtheme != value)
                {
                    selectedtheme = value;
                    OnPropertyChanged("SelectedTheme");
                }
            }

        }
        #endregion

    }
}
