using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amatsukaze.ViewModel
{
    class FolderEntity
    {
        // Wait, everything has to be public ?!
        public string name { get; set; }
        public string originalName { get; set; }
        public string path { get; set; }
        public string imagePath { get; set; }
        public string synopsis { get; set; }
        public string date { get; set; }
    }


}
