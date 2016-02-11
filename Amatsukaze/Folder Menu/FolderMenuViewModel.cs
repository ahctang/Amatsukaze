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
using System.ComponentModel;

namespace Amatsukaze.ViewModel
{
    class FolderMenuViewModel : ObservableObjectClass, ViewModelBase, INotifyPropertyChanged
    {
        private ObservableCollection<FolderEntity> folders = new ObservableCollection<FolderEntity>();
        private ObservableCollection<FolderItem> folderContents = new ObservableCollection<FolderItem>();
        private FolderEntity selectedFolder;

        private ICommand displaySelectFolderDialog;
        private ICommand displayDeleteDialog;
        //private Boolean isSelectDialogOpen;
        

        public FolderMenuViewModel(IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            readFolders();
        }

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
                Console.WriteLine("Added folder " + newFolder.name);

                saveFolders(folders);
            }
            else
            {
                // TODO: print error message on displayed console
            }
        }

        private void openDeleteDialog()
        {
            var confirmResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to remove the [" + selectedFolder.name + "] folder?\nThis will not delete the folder from your hard drive.",
                                     "Remove",
                                     MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                Console.WriteLine("Removed folder " + selectedFolder.name);
                folders.Remove(selectedFolder);
                saveFolders(folders);
            }
            else
            {
                // Close dialog and do nothing
            }
        }

        private void openOrganizeDialog()
        {

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

        private void onFolderSelection()
        {
            if (selectedFolder != null)
            {
                //selectedFolderNameTextBlock.Text = selectedFolder.name;
                DirectoryInfo info = new DirectoryInfo(selectedFolder.path);
                ObservableCollection<FolderItem> results = new ObservableCollection<FolderItem>();

                foreach (DirectoryInfo directoryInfo in info.GetDirectories())
                {
                    FolderItem item = new FolderItem();
                    item.name = directoryInfo.Name;
                    item.type = "D";
                    item.contents = "contains " + directoryInfo.EnumerateFiles().Count() + " files and "
                        + directoryInfo.EnumerateDirectories().Count() + " sub-directories";
                    results.Add(item);
                }

                foreach (FileInfo fileInfo in info.GetFiles())
                {
                    FolderItem item = new FolderItem();
                    item.name = fileInfo.Name;
                    item.type = "F";
                    results.Add(item);
                }
                FolderContents = results;
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
                Console.WriteLine("blop");
                OnPropertyChanged("Folders");
            }
        }

        public FolderEntity SelectedFolder
        {
            get
            {
                return selectedFolder;
            }
            set
            {
                selectedFolder = value;
                Console.WriteLine("nya");
                OnPropertyChanged("SelectedFolder");
                onFolderSelection();
            }
        }

        public ObservableCollection<FolderItem> FolderContents
        {
            get
            {
                return folderContents;
            } set
            {
                folderContents = value;
                OnPropertyChanged("FolderContents");
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
                            p => openAddFolderDialog(),
                            p => true
                        );
                }
                return displaySelectFolderDialog;
            }
        }

        public ICommand DeleteSelectedFolderDialog
        {
            get
            {
                if (displayDeleteDialog == null)
                {
                    displayDeleteDialog = new RelayCommand
                        (
                            p => openDeleteDialog(),
                            p => true
                        );
                }
                return displayDeleteDialog;
            }
        }

        public string BaseName
        {
            get
            {
                return "Folder Menu";
            }
        }

        public class FolderItem
        {
            public string name { get; set; }
            public string type { get; set; }
            public string contents { get; set; }
        }

    }
}
