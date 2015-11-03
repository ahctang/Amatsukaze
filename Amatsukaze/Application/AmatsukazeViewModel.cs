using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;
using System.ComponentModel;
using System.Windows.Input;

namespace Amatsukaze.ViewModel
{

    //This viewmodel serves as the datacontext for MainWindowView and directly controls all of the view switching and perhaps view data sharing.
    //Any new top level viewmodel probably needs to be linked up here

    class AmatsukazeViewModel: ObservableObjectClass
    {
        #region fields
        private ICommand _changeViewCommand;

        private string currentview;
        private ViewModelBase _currentviewmodel;
        private List<ViewModelBase> _ApplicationViewModels;

        #endregion


        public AmatsukazeViewModel()
        {            
            ApplicationViewModels.Add(new LibraryMenuViewModel());
            ApplicationViewModels.Add(new FolderMenuViewModel());
            ApplicationViewModels.Add(new SocialNetworkMenuViewModel());            
            ApplicationViewModels.Add(new PlaybackMenuViewModel());
            ApplicationViewModels.Add(new OptionMenuViewModel());

            CurrentViewModel = ApplicationViewModels[0];
            CurrentView = CurrentViewModel.GetType().ToString();
        }

        #region Properties/Commmands

        public ICommand ChangeViewCommand
        {
            get
            {
                if (_changeViewCommand == null)
                {
                    _changeViewCommand = new RelayCommand(
                        p => ChangeViewModel((ViewModelBase)p),
                        p => p is ViewModelBase);
                }
                return _changeViewCommand;
            }
        }

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

        public string CurrentView
        {
            get
            {
                return currentview;
            }
            set
            {
                if (currentview != value)
                {
                    currentview = value;
                    OnPropertyChanged("CurrentView");
                }
            }

        }

        #endregion

        #region Methods

        private void ChangeViewModel(ViewModelBase viewModel)
        {
            if (!ApplicationViewModels.Contains(viewModel))
                ApplicationViewModels.Add(viewModel);

            CurrentViewModel = ApplicationViewModels.FirstOrDefault(vm => vm == viewModel);
            this.CurrentView = viewModel.GetType().ToString();
        }        
        #endregion
    }
}
