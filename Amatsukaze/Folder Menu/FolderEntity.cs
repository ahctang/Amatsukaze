using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amatsukaze.ViewModel
{
    /**
    * This represents a folder. It is displayed on the left of the folder view.
    **/
    class FolderEntity
    {
        public string name { get; set; }
        public string originalName { get; set; }
        public string path { get; set; }
        public string imagePath { get; set; }
        public string synopsis { get; set; }
        public string date { get; set; }
    }

    /**
    * This represents the content of a folder.
    **/
    public class FolderItem
    {
        public string name { get; set; }
        public string type { get; set; }
        public string contents { get; set; }
    }

}
