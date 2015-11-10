using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;
using Amatsukaze.Model;
using System.Collections;
using System.Collections.ObjectModel;

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

        public LibraryMenuViewModel()
        {
            LibraryMenuModel datasource = new LibraryMenuModel();
            datasource.ReadXMLDirectory();
            datasource.ReadCacheFile();

            animelibrarylist = new ObservableCollection<AnimeEntryObject>(datasource.AnimeLibraryList);
        }

        #region Objects

        private ObservableCollection<AnimeEntryObject> animelibrarylist;
        #endregion

        #region Properties
        public ObservableCollection<AnimeEntryObject> AnimeLibraryList
        {
            get
            {
                return animelibrarylist;
            }
        }
        #endregion
    }
}
