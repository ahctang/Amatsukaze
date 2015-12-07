using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace Amatsukaze.ViewModel
{
    class FolderMenuViewModel : ObservableObjectClass, ViewModelBase
    {
        private ObservableCollection<FolderEntity> folders = new ObservableCollection<FolderEntity>();

        private ICommand displaySelectFolderDialog;
        //private Boolean isSelectDialogOpen;

        public FolderMenuViewModel(IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            readFolders();
        }

        // TODO: Put these methods in view model
        private void openAddFolderDialog()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.ShowDialog();

            string selected = dialog.SelectedPath;

            if (!folderListContainsByPath(selected))
            {
                FolderEntity newFolder = new FolderEntity();
                newFolder.name = selected.Substring(selected.LastIndexOf("\\") + 1);
                newFolder.path = selected;
                folders.Add(newFolder);

                saveFolders(folders);
            }
            else
            {
                // TODO: print error message on displayed console
            }
        }

        private Boolean folderListContainsByPath(string path)
        {
            foreach (FolderEntity folder in folders)
            {
                if (path.Equals(folder.path))
                {
                    return true;
                }
            }
            return false;
        }

        private void readFolders()
        {
            string folderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Preferences\";
            string filepath = folderpath + @"\folders.json";

            if (File.Exists(filepath))
            {
                string input = File.ReadAllText(filepath);
                ObservableCollection<FolderEntity> folderList = JsonConvert.DeserializeObject<ObservableCollection<FolderEntity>>(input);
                // FIXME: At this point, data context is not initialized, see if it works in view model
                folders = folderList;
            }
        }

        private void saveFolders(ObservableCollection<FolderEntity> folderList)
        {
            string json = JsonConvert.SerializeObject(folderList, Formatting.Indented);
            try
            {
                string folderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Preferences\";
                string filepath = folderpath + @"\folders.json";

                FileInfo folder = new FileInfo(folderpath);
                folder.Directory.Create();
                File.WriteAllText(filepath, json);
            }
            catch (Exception exception)
            {
                //FileIO exception
                System.Windows.MessageBox.Show(exception.Message);
            }
        }

        public IEventAggregator EventAggregator { get; set; }

        public ObservableCollection<FolderEntity> Folders
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

        public ICommand DisplaySelectFolderDialog
        {
            get
            {
                if (displaySelectFolderDialog == null)
                {
                    displaySelectFolderDialog = new RelayCommand
                        (
                            p => openAddFolderDialog()
                            //p => isSelectDialogOpen
                        );
                }
                return displaySelectFolderDialog;
            }
        }

        public string BaseName
        {
            get
            {
                return "Folder Menu";
            }
        }
    }
}
