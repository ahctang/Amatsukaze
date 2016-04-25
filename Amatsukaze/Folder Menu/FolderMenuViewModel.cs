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
        #region Fields
        
        /**********************
        ***** Folder View *****
        **********************/

        // Folders, displayed in the list on the left
        private ObservableCollection<FolderEntity> folders = new ObservableCollection<FolderEntity>();
        // The selected folder
        private FolderEntity selectedFolder;
        // Selected folder contents, displayed on bottom right
        private ObservableCollection<FolderItem> folderContents = new ObservableCollection<FolderItem>();

        /************************
        ***** Organize View *****
        ************************/

        // Number of series in the selected folder
        private string seriesCount;
        // Series in the selected folder
        private ObservableCollection<Series> series = new ObservableCollection<Series>();
        // The selected series
        private Series selectedSeries;
        // Episode names in the selected series
        private string episodesNames;

        /*******************
        ***** Commands *****
        *******************/

        // Display the dialog that lets the user add a folder
        private ICommand displaySelectFolderDialog;
        // Display the dialog that lets the user delete a folder
        private ICommand displayDeleteDialog;

        #endregion

        #region methods

        /// <summary>
        /// Constructor, reads the json file containing saved folders at instanciation.
        /// </summary>
        /// <param name="eventAggregator">eventAggregator of the ViewModel</param>
        public FolderMenuViewModel(IEventAggregator eventAggregator)
        {
            this.EventAggregator = eventAggregator;
            readFolders();
        }

        /// <summary>
        /// Trigerred when clicking the <add> button on Folder page.
        /// This opens a dialog letting the user select a folder to add.
        /// </summary>
        private void openAddFolderDialog()
        {
            // Open the dialog
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.ShowDialog();

            // The selected folder is added to saved folders in json
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
                Console.WriteLine("Warning: folder " + selected + "already exists in json file.");
            }
        }

        /// <summary>
        /// Trigerred when clicking the <delete> button on Folder page.
        /// This opens a dialog prompting the user to confirm deletion of folder in json.
        /// </summary>
        private void openDeleteDialog()
        {
            var confirmResult = System.Windows.Forms.MessageBox.Show(
                "Are you sure you want to remove the [" + selectedFolder.name + 
                    "] folder?\nThis will not delete the folder from your hard drive.",
                "Remove",
                MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                Console.WriteLine("Removed folder " + selectedFolder.name);
                folders.Remove(selectedFolder);
                saveFolders(folders);
            }
        }

        // TODO
        private void openOrganizeDialog()
        {

        }

        /// <summary>
        /// Checks if the folder is already in the json file.
        /// </summary>
        /// <param name="path">Path of folder to check</param>
        /// <returns></returns>
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

        /// <summary>
        /// Reads the json file containing all saved folders.
        /// </summary>
        private void readFolders()
        {
            // TODO : Make json file path a constant
            string folderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Preferences\";
            string filepath = folderpath + @"\folders.json";

            if (File.Exists(filepath))
            {
                string input = File.ReadAllText(filepath);
                ObservableCollection<FolderEntity> folderList = JsonConvert.DeserializeObject<ObservableCollection<FolderEntity>>(input);
                folders = folderList;
            }
        }

        /// <summary>
        /// Save all selected folders into the json file.
        /// </summary>
        /// <param name="folderList"></param>
        private void saveFolders(ObservableCollection<FolderEntity> folderList)
        {
            string json = JsonConvert.SerializeObject(folderList, Formatting.Indented);
            try
            {
                // TODO : make json file path a constant.
                string folderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Preferences\";
                string filepath = folderpath + @"\folders.json";

                FileInfo folder = new FileInfo(folderpath);
                folder.Directory.Create();
                File.WriteAllText(filepath, json);
            }
            catch (Exception exception)
            {
                //TODO : FileIO exception to handle
                System.Windows.MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        /// Trigerred when selecting a folder in the list.
        /// This gets information on the folder for display, and fills the "series" global variable
        /// for external use
        /// </summary>
        private void onFolderSelection()
        {
            // Display information about the folder
            if (selectedFolder != null && selectedFolder.path != null && selectedFolder.path != "")
            {
                DirectoryInfo info = new DirectoryInfo(selectedFolder.path);
                ObservableCollection<FolderItem> results = new ObservableCollection<FolderItem>();

                // Display subfolders informations in the selected folder
                foreach (DirectoryInfo directoryInfo in info.GetDirectories())
                {
                    FolderItem item = new FolderItem();
                    item.name = directoryInfo.Name;
                    item.type = "D";
                    item.contents = "contains " + directoryInfo.EnumerateFiles().Count() + " files and "
                        + directoryInfo.EnumerateDirectories().Count() + " sub-directories";
                    results.Add(item);
                }

                // Display file informations in the selected folder
                foreach (FileInfo fileInfo in info.GetFiles())
                {
                    FolderItem item = new FolderItem();
                    item.name = fileInfo.Name;
                    item.type = "F";
                    results.Add(item);
                }
                // Next line does the display
                FolderContents = results;

                // Fills the series global variable for further use
                series = FileOrganizerLogic.parseAsSeries(selectedFolder.path);
                OnPropertyChanged("Series");

                // Fill informations to display in the folder OrganizeView
                seriesCount = "Found " + series.Count() + " series in folder";
                // TODO : lots of stuff to add
            }
        }

        #endregion

        #region Properties

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
                OnPropertyChanged("SelectedFolder");

                onFolderSelection();
                OnPropertyChanged("SeriesCount");
            }
        }

        public ObservableCollection<FolderItem> FolderContents
        {
            get
            {
                return folderContents;
            }
            set
            {
                folderContents = value;
                OnPropertyChanged("FolderContents");
            }
        }

        public string SeriesCount
        {
            get
            {
                return seriesCount;
            }
            set
            {
                seriesCount = value;
            }
        }

        public ObservableCollection<Series> Series
        {
            get
            {
                return series;
            }
            set
            {
                OnPropertyChanged("Series");
                series = value;
            }
        }

        public Series SelectedSeries
        {
            get
            {
                return selectedSeries;
            }
            set
            {
                selectedSeries = value;
                OnPropertyChanged("SelectedSeries");
            }
        }

        public string EpisodesNames
        {
            get
            {
                return episodesNames;
            }
            set
            {
                episodesNames = value;
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

        #endregion
    }
}
