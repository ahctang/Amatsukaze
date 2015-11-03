﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;
using System.ComponentModel;
using System.Windows.Input;

namespace Amatsukaze.ViewModel
{

    //This viewmodel serves as the datacontext for MainWindow and directly controls all of the view switching and perhaps view data sharing.
    //Any new top level viewmodel probably needs to be linked up here

    class AmatsukazeViewModel: ObservableObjectClass
    {
        #region fields
        private ICommand _changePageCommand;

        private ViewModelBase _currentviewmodel;
        private List<ViewModelBase> _ApplicationViewModels;

        #endregion


        public AmatsukazeViewModel()
        {            
            ApplicationViewModels.Add(new LibraryMenuViewModel());
            ApplicationViewModels.Add(new FolderMenuViewModel());
            ApplicationViewModels.Add(new OptionMenuViewModel());
            ApplicationViewModels.Add(new PlaybackMenuViewModel());
            ApplicationViewModels.Add(new SocialNetworkMenuViewModel());

            CurrentViewModel = ApplicationViewModels[0];
        }

        #region Properties/Commmands

        public List<ViewModelBase> ApplicationViewModels
        {
            get
            {
                if (_ApplicationViewModels == null)
                    _ApplicationViewModels = new List<ViewModelBase>();
                return _ApplicationViewModels;
            }
        }

        public ViewModelBase CurrentViewModel
        {
            get
            {
                return _currentviewmodel;
            }
            set
            {
                if(_currentviewmodel != value)
                {
                    _currentviewmodel = value;
                    OnPropertyChanged("CurrentViewModel");
                }
            }
        }
            
        #endregion


    }
}
