using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amatsukaze.ViewModel
{
    /// <summary>
    /// Logic for reading the folder json file.
    /// </summary>
    public static class FolderReadingLogic
    {
        #region Fields

        /// <summary>Directory that contains the json files</summary>
        private static readonly string JSON_DIRECTORY = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Preferences\";
        /// <summary>File that contains the saved folders</summary>
        private static readonly string FOLDER_JSON_FILE = JSON_DIRECTORY + @"\folders.json";

        #endregion

        #region Methods

        /// <summary>
        /// Reads the json file containing all saved folders.
        /// </summary>
        public static ObservableCollection<FolderEntity> readFoldersCache()
        {
            if (File.Exists(FOLDER_JSON_FILE))
            {
                string input = File.ReadAllText(FOLDER_JSON_FILE);
                ObservableCollection<FolderEntity> result = JsonConvert.DeserializeObject<ObservableCollection<FolderEntity>>(input);
                return result;
            }
            else
            {
                return new ObservableCollection<FolderEntity>();
            }
        }

        /// <summary>
        /// Save the folders in parameter into the json file.
        /// </summary>
        /// <param name="folders">folders to save</param>
        public static void saveToFoldersCache(ObservableCollection<FolderEntity> folders)
        {
            try
            {
                // Serialize object as json
                string json = JsonConvert.SerializeObject(folders, Formatting.Indented);

                // If save directory doesn't exist, create it
                if (!Directory.Exists(JSON_DIRECTORY))
                {
                    new DirectoryInfo(JSON_DIRECTORY).Create();
                }
                // If save file doesn't exist, create it
                if (!File.Exists(FOLDER_JSON_FILE))
                {
                    File.Create(FOLDER_JSON_FILE);
                }
                // Write data to the file
                File.WriteAllText(FOLDER_JSON_FILE, json);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        #endregion

        #region Internal classes

        /// <summary>
        /// This represents a folder. It is displayed on the left of the folder view.
        /// </summary>
        public class FolderEntity
        {
            /// <summary>Name of the folder</summary>
            public string name { get; set; }
            /// <summary>Absolute path to the folder</summary>
            public string path { get; set; }
        }

        /// <summary>
        /// This represents the contents of a folder, it can either be a file or a subfolder.
        /// </summary>
        public class FolderItem
        {
            /// <summary>Name of the Item</summary>
            public string name { get; set; }
            /// <summary>F if the item is a file or D if it's a directory</summary>
            public string type { get; set; }
            /// <summary>Comment about the contents of this Item if a folder for display in Folder View</summary>
            public string contents { get; set; }
        }

        #endregion

    }
}
