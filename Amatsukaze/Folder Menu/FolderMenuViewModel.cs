using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace Amatsukaze.ViewModel
{
    class FolderMenuViewModel : ObservableObjectClass, ViewModelBase
    {

        private ObservableCollection<string> folders;
        public ObservableCollection<string> Folders
        {
            get
            {
                return folders;
            }
            set
            {
                folders = value;
            }
        }

        private string test;

        public string Test
        {
            get
            {
                if (test == null)
                    test = "blop";
                return test;
            }
            set
            {
                if (test != value)
                {
                    test = value;
                    OnPropertyChanged("Test");
                }
            }
        }

        public string BaseName
        {
            get
            {
                return "Folder Menu";
            }
            /*set
            {
                if (BaseName != value)
                BaseName = value;
                OnPropertyChanged(BaseName);
            }*/
        }
    }
}
