using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Amatsukaze.ViewModel
{

    //Static object for holding all of the user preferences if they are to be used by other parts of the application
    class OptionsObject
    {
        public string Themesetting {get; set;}
        public string CacheFolderpath { get; set; }

        //Sets the options object to default values.
        public void SetDefaults()
        {
            this.Themesetting = "Amatsukaze";
            this.CacheFolderpath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Cache\";
        }
    }
}
