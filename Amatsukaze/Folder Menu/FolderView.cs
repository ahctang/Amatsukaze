﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;

namespace Amatsukaze.ViewModel
{
    class FolderMenuViewModel : ObservableObjectClass, ViewModelBase
    {
        public string BaseName
        {
            get
            {
                return "Folder Menu";
            }
        }
    }
}
