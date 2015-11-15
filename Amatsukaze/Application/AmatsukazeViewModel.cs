using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.HelperClasses;
using System.Windows;
using System.Windows.Input;
using Amatsukaze.Model;
using System.Timers;

namespace Amatsukaze.ViewModel
{

    //This viewmodel serves as the datacontext for MainWindowView and directly controls all of the view switching and perhaps view data sharing.
    //Any new top level viewmodel probably needs to be linked up here

    class AmatsukazeViewModel : ObservableObjectClass, ISubscriber<MessagetoGUI>
    {
        public AmatsukazeViewModel()
        {
            //Initialize new global instance of eventaggregator to be passed via Dependency Injection (<--- funniest term I have ever heard) to every new viewmodel instance
            this.eventAggregator = new EventAggregator();

            //Subscrible to all events in the eventaggregator
            eventAggregator.SubscribeEvent(this);


            //Read the Options into the application memory before doing anything
            OptionsModel optionsmodel = new OptionsModel();
            optionsmodel.ReadOptionsFile(ref optionsobject);
            ApplyTheme();

            //Instantiate the viewmodels for the application
            ApplicationViewModels.Add(new LibraryMenuViewModel(optionsobject, eventAggregator));
            ApplicationViewModels.Add(new FolderMenuViewModel());
            ApplicationViewModels.Add(new SocialNetworkMenuViewModel());
            ApplicationViewModels.Add(new PlaybackMenuViewModel());
            ApplicationViewModels.Add(new OptionMenuViewModel(optionsobject, eventAggregator));

            CurrentViewModel = ApplicationViewModels[0];
            CurrentView = CurrentViewModel.GetType().ToString();
        }

        #region Fields
        private string statustext;
        private bool messageTextToggle;
        private Timer t = new Timer(3000);

        private ICommand _changeViewCommand;

        private string currentview;
        private ViewModelBase _currentviewmodel;

        //Private list that contains every viewmodel.
        private List<ViewModelBase> _ApplicationViewModels;

        #endregion
        #region Objects
        OptionsObject optionsobject = new OptionsObject();

        //Event aggregator for collecting messages from all viewmodels and forwarding them to the GUI.
        public IEventAggregator eventAggregator { get; set; }
        #endregion

       

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
                if (_currentviewmodel != value)
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

        public string StatusText
        {
            get
            {
                return statustext;
            }
            set
            {
                if (statustext != value)
                {
                    statustext = value;
                    OnPropertyChanged("StatusText");
                }
            }
        }

        public bool MessageTextToggle
        {
            get
            {
                return messageTextToggle;
            }
            set
            {
                if (messageTextToggle != value)
                {
                    messageTextToggle = value;
                    OnPropertyChanged("MessageTextToggle");
                }
            }
        }

        #endregion

        #region Methods

        public void OnEventHandler(MessagetoGUI sender)
        {
            Console.WriteLine("Amatsukaze Event Fired!");
            //Reset the message timer
            t.Stop();
            t.Start();
            
            string message = sender.Message;
            StatusText = message;            

            //Show the message panel
            MessageTextToggle = true;

            //Go to event handler that closes the message panel
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Stop();
            MessageTextToggle = false;
        }

        private void ChangeViewModel(ViewModelBase viewModel)
        {
            if (!ApplicationViewModels.Contains(viewModel))
                ApplicationViewModels.Add(viewModel);

            CurrentViewModel = ApplicationViewModels.FirstOrDefault(vm => vm == viewModel);
            this.CurrentView = viewModel.GetType().ToString();
        }

        private void ApplyTheme()
        {
            string input = @"/Resources/" + optionsobject.Themesetting + ".xaml";
            Uri uri1;

            if (Uri.TryCreate(input, UriKind.Relative, out uri1))
            {
                var app = Application.Current as App;
                app.ChangeTheme(uri1);
            }            
        }        

        #endregion

      
    }
}
