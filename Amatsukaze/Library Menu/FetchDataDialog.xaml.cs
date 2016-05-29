using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amatsukaze.HelperClasses;

namespace Amatsukaze.View
{
    /// <summary>
    /// Interaction logic for FetchDataDialog.xaml
    /// </summary>
    public partial class FetchDataDialog : UserControl
    {
        public FetchDataDialog()
        {
            InitializeComponent();
        }

        private void PopulateAnimeNameTextBox(object sender, PopulatingEventArgs e)
        {
            string text = AnimeNameTextBox.Text;

            List<Item> searchResults = MALAccessor.searchAnimeByName(text);
            AnimeNameTextBox.ItemsSource = searchResults;
            AnimeNameTextBox.PopulateComplete();
        }
    }
}
