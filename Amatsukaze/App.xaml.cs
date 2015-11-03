using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Amatsukaze.View;
using Amatsukaze.ViewModel;

namespace Amatsukaze
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindowView app = new MainWindowView();
            AmatsukazeViewModel datacontext = new AmatsukazeViewModel();
            app.DataContext = datacontext;
            app.Show();
            
        }
    }
}
