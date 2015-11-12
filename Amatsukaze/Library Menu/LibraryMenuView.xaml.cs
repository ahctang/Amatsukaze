using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amatsukaze.ViewModel;


namespace Amatsukaze.View
{
    /// <summary>
    /// Interaction logic for LibraryMenuView.xaml
    /// </summary>
    public partial class LibraryMenuView : UserControl
    {
        
        public LibraryMenuView()
        {
            InitializeComponent();              
        }        

        #region Event handlers

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {      
            //For reassigning all of the indexes whenever the window is changed
            var datacontext = DataContext as LibraryMenuViewModel;
            int columncount = (int)DisplayArea.ActualWidth / 180;
            datacontext.DisplayAreaResized(columncount);                        
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //For Initializing the Grid based on the usercontrol width
            var datacontext = DataContext as LibraryMenuViewModel;
            int columncount = (int)DisplayArea.ActualWidth / 180;
            datacontext.DisplayAreaResized(columncount);
        }
        #endregion

        #region objects

        #endregion

        private void ActivityLog_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = DataContext as LibraryMenuViewModel;
            switch (datacontext.MessageLogToggle)
            {
                case true:
                    datacontext.MessageLogToggle = false;
                    break;
                case false:
                    datacontext.MessageLogToggle = true;
                    break;
            }
        }
    }

    public class UnsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = value as string;
            if (input == "") return DependencyProperty.UnsetValue;
            else return input;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageBoxWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            var input = value as double?;
            input = input * 75 / 100;
            return input;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GridResizedEventArgs : EventArgs
    {
        public int Columncount { get; set; }
    }    
}
