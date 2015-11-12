using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Amatsukaze.View
{
    /// <summary>
    /// Interaction logic for OptionsMenuView.xaml
    /// </summary>
    public partial class OptionsMenuView : UserControl
    {
        public OptionsMenuView()
        {
            InitializeComponent();            
        }

        #region Events
        private void ThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (e.AddedItems.Count != 0)
            {
                Console.WriteLine(e.AddedItems[0]);
                string input = @"/Resources/" + e.AddedItems[0] as string + ".xaml";
                Uri uri1;

                if (Uri.TryCreate(input, UriKind.Relative, out uri1))
                {
                    var app = Application.Current as App;
                    app.ChangeTheme(uri1);
                }                            
            }
        }

        #endregion

        #region Methods

        
        #endregion
    }
}
