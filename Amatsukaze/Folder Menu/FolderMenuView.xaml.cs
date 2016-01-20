using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Amatsukaze.ViewModel;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Reflection;

namespace Amatsukaze.View
{
    /// <summary>
    /// Interaction logic for FolderMenuView.xaml
    /// </summary>
    public partial class FolderMenuView : System.Windows.Controls.UserControl
    {
        //TODO : Be clean and remove code from code-behind

        public FolderMenuView()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FolderEntity selectedFolder = (FolderEntity) folderListBox.SelectedItem;
            if (selectedFolder != null)
            {
                selectedFolderNameTextBlock.Text = selectedFolder.name;
                DirectoryInfo info = new DirectoryInfo(selectedFolder.path);
                List<FolderItem> results = new List<FolderItem>();

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
                SelectedFolderContentsControl.ItemsSource = results;
            }
        }
    }

    public class FolderItem
    {
        public string name { get; set; }
        public string type { get; set; }
        public string contents { get; set; }
    }
}
