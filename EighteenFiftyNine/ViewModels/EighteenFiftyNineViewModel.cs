using EighteenFiftyNine.Models.Classes;
using EighteenFiftyNine.Models.Interfaces;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace EighteenFiftyNine.ViewModels
{
   public class EighteenFiftyNineViewModel : ViewModelBase
   {
      #region Fields
      private ICommand _toggleLED = null;
      private ICommand _initializeLEDCommand = null;
      private ICommand _shutdownLEDCommand = null;

      private ICommand _toggleStagePower = null;
      private ICommand _initializeStageCommand = null;
      private ICommand _shutdownStageCommand = null;

      private ICommand _validationCommand = null;

      private ILight _lightModel;
      private IStage _stageModel;

      private bool _validationRoutineRunning = false;


      private ObservableCollection<StationDescription> _stationDescriptions = new ObservableCollection<StationDescription>();
      private StationDescription _selectedStationDescription = new StationDescription();

      CancellationTokenSource _tokenSource;
      CancellationToken _ct;

      string _validationText = "Run Validation";
      #endregion Fields
      #region Constructor

      public EighteenFiftyNineViewModel(IStage stageModel, ILight lightModel)
      {
         _stageModel = stageModel;
         _lightModel = lightModel;
         ValidationText = "Run Validation";  // TODO: DON'T Hardcode!
         StationDescriptions.Add(new StationDescription() { Description = "Home", StationNumber = 0 });
         StationDescriptions.Add(new StationDescription() { Description = "Station 1", StationNumber = 1 });
         StationDescriptions.Add(new StationDescription() { Description = "Station 2", StationNumber = 2 });
         StationDescriptions.Add(new StationDescription() { Description = "Station 3", StationNumber = 3 });
         StationDescriptions.Add(new StationDescription() { Description = "Station 4", StationNumber = 4 });
         StationDescriptions.Add(new StationDescription() { Description = "Station 5", StationNumber = 5 });
         StationDescriptions.Add(new StationDescription() { Description = "Station 6", StationNumber = 6 });
         StationDescriptions.Add(new StationDescription() { Description = "Station 7", StationNumber = 7 });
         StationDescriptions.Add(new StationDescription() { Description = "Station 8", StationNumber = 8 });
         StationDescriptions.Add(new StationDescription() { Description = "Station 9", StationNumber = 9 });
         StationDescriptions.Add(new StationDescription() { Description = "Station 10", StationNumber = 10 });

         // force an ininitial update of combobox        
         OnPropertyChanged("SelectedStationDescription");
         OnPropertyChanged("AngleProperty");

         _stageModel.PropertyChanged += _stageModel_PropertyChanged;
         _lightModel.PropertyChanged += _lightModel_PropertyChanged;

      }

      #endregion

      #region Properties

      public string ValidationText
      {
         get
         {
            return _validationText;
         }
         set
         {
            _validationText = value;
            OnPropertyChanged("ValidationText");
         }
      }
      public SolidColorBrush LEDFillColor
      {
         get
         {
            Color color = Colors.Green;
            color.A = _lightModel.On ? _lightModel.Intensity : (byte)0;
            return new SolidColorBrush(color);
         }
      }

      public String LEDIntensity
      {
         get
         {
            return _lightModel.Intensity.ToString();
         }
         set
         {
            _lightModel.Intensity = Convert.ToByte(value);
            OnPropertyChanged("LEDIntensity");
            OnPropertyChanged("LEDFillColor");
         }
      }



      public SolidColorBrush LEDSafetyColor
      {
         get => _lightModel.On ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
      }

      public String LEDText
      {
         get => _lightModel.On ? "LED Power is On!" : "LED Power Off";
      }

      public bool LEDOn
      {
         get => _lightModel.On;
      }

      public SolidColorBrush StageSafetyColor
      {
         get => _stageModel.On ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
      }

      public string StagePowerText
      {
         get => _stageModel.On ? "Stage Power is On!" : "Stage Power Off";
      }

      public bool StagePowerOn
      {
         get => _stageModel.On;
      }

      public bool ValidationProcedureEnabled
      {
         get => _validationRoutineRunning || (_stageModel.On && _lightModel.On);
      }

      public ObservableCollection<StationDescription> StationDescriptions
      {
         get { return _stationDescriptions; }
         set { _stationDescriptions = value; OnPropertyChanged("StationDescriptions"); }
      }

      public StationDescription SelectedStationDescription
      {
         get
         {
            StationDescription stationDescription = StationDescriptions.First(p => p.StationNumber == _stageModel.Position);
            if (stationDescription == null)
            {
               // TODO Log error and take appropriate action. Perhaps throw, as this should not happen.

            }
            return stationDescription;
         }
         set
         {

            Task.Run(() => _stageModel.RequestStagePositionChange(value.StationNumber));

         }
      }

      public double AngleProperty
      {
         get
         {
            Debug.WriteLine("AngleProperty _stageModel.Position: " + _stageModel.Position);
            return 360.0 / StationDescriptions.Count * _stageModel.Position;
         }


      }
      #endregion //properties



      #region ICommmand and implementations
      public ICommand ToggleLED
      {
         get
         {
            if (_toggleLED == null)
            {
               _toggleLED = new RelayCommand(param => ToggleLEDImplementation());
            }
            return _toggleLED;
         }
      }

      public ICommand InitializeLEDCommand
      {
         get
         {
            if (_initializeLEDCommand == null)
            {
               _initializeLEDCommand = new RelayCommand(param => InitializeLEDCommandImplementation());
            }
            return _initializeLEDCommand;
         }
      }

      public ICommand ShutdownLEDCommand
      {
         get
         {
            if (_shutdownLEDCommand == null)
            {
               _shutdownLEDCommand = new RelayCommand(param => ShutdownLEDCommandImplementation());
            }
            return _shutdownLEDCommand;
         }
      }


      public ICommand ToggleStagePower
      {
         get
         {
            if (_toggleStagePower == null)
            {
               _toggleStagePower = new RelayCommand(param => ToggleStagePowerImplementation());
            }
            return _toggleStagePower;
         }
      }

      public ICommand InitializeStageCommand
      {
         get
         {
            if (_initializeStageCommand == null)
            {
               _initializeStageCommand = new RelayCommand(param => InitializeStageCommandImplementation());
            }
            return _initializeStageCommand;
         }
      }

      public ICommand ShutdownStageCommand
      {
         get
         {
            if (_shutdownStageCommand == null)
            {
               _shutdownStageCommand = new RelayCommand(param => ShutdownStageCommandImplementation());
            }
            return _shutdownStageCommand;
         }
      }

      public ICommand ValidationCommand
      {
         get
         {
            if (_validationCommand == null)
            {
               _validationCommand = new RelayCommand(param => ValidationCommandImplementation());
            }
            return _validationCommand;
         }
      }
      private void InitializeLEDCommandImplementation()
      {
         _lightModel.On = false;  // turn off, then set intensity
         _lightModel.Intensity = 50; // seems like a nice safe light value to start at!
         _lightModel.On = true;


         UpdateAllLEDControls();

         Task myTask = Task.Run(() =>
        {
           while (_lightModel.Intensity < 200)
           {

              _lightModel.Intensity += 35;

              Thread.Sleep(500); // just a little time to see it

           }
        });
         myTask.ContinueWith(task =>
         {
            _lightModel.Intensity = 50;
         });


         // TODO: Toast notification that initialization is complete
      }

      private void ShutdownLEDCommandImplementation()
      {
         _lightModel.On = false;
         _lightModel.Intensity = 50; // set to "safe value" for next time
         UpdateAllLEDControls();
      }


      private void ToggleLEDImplementation()
      {
         _lightModel.On = !_lightModel.On;
         UpdateAllLEDControls();
      }

      private void ToggleStagePowerImplementation()
      {


         if (_tokenSource != null)
         {
            _tokenSource.Cancel();
            // Very much a hack until I fix the sockets I introduced :)
            Thread.Sleep(2000); // we assume there is just 1 move to make. Not true until I fix "Home" method.
         }

         _stageModel.On = !_stageModel.On;
         UpdateAllStageControls();
      }

      private async void InitializeStageCommandImplementation()
      {
         // let's assume "Initialization means turn on, Home, visit each station and return to home!

         _tokenSource = new CancellationTokenSource();
         _ct = _tokenSource.Token;

         _stageModel.On = true;
         await _stageModel.Home(); // should make this part cancellable too! Depending where we are, Home() could take a long time!
         for (uint i = 1; i < 11; i++)
         {
            if (_ct != null && _ct.IsCancellationRequested)
               return;

            await _stageModel.RequestStagePositionChange(i);
         }
         if (_ct != null && _ct.IsCancellationRequested)
            return;
         await _stageModel.Home();


      }

      private async void ShutdownStageCommandImplementation()
      {
         if (_tokenSource != null)
         {
            _tokenSource.Cancel();
            // Very much a hack just until I fix the sockets I introduced :)
            await Task.Delay(2000);// we assume there is just 1 move to make. Not true until I fix "Home" method.
         }
         await _stageModel.Home(); // safest place for stage
         _stageModel.On = false;
         UpdateAllStageControls();
      }


      /// <summary>
      /// TODO: Cancellation needs a lot of work. It's not responding reliably on first click AND it's very hacky in any case. Re-think!
      /// </summary>
      internal async Task ValidationCommandImplementation() // made internal for unit testing purposes. I suspect there are slicker ways...
      {

         try
         {

            _validationRoutineRunning = true;
            if (ValidationText == "Cancel Validation")  // I'm aware this is a very "hacky" implementation of Cancel. Ran out of time to think more!
            {
               if (_tokenSource != null)
                  _tokenSource.Cancel();
               return;
            }
            await _stageModel.Home();  // TODO: Allow user to cancel during Home part too! Might want cancellation stuff at class level.

            _tokenSource = new CancellationTokenSource();
            _ct = _tokenSource.Token;
            ValidationText = "Cancel Validation";

            for (uint i = 1; i <= 10; i++)
            {
               if (_ct != null && _ct.IsCancellationRequested)
               {
                  // just stop. User may have to initialize again, but c'est la vie.
                  ValidationText = "Run Validation";
                  _stageModel.On = false;   // turn off power.  Make them really think about it, since they just cancelled!
                  return;
               }
               _lightModel.On = true;
               await Task.Delay(500); // Allow GUI to rerender. TODO: A better way?
               await _stageModel.RequestStagePositionChange(i);

               _lightModel.On = false;
               await Task.Delay(500); // Allow GUI to rerender. TODO: A better way?
                                      // Sleep between each move for 1 second (per requirements)
               if (i < 10)
               {
                  await Task.Delay(1000);
                  //Thread.Sleep(1000);
               }


               double newIntensity = _lightModel.Intensity * 1.10;
               if (newIntensity <= 255)
                  _lightModel.Intensity = (byte)newIntensity;
               else
                  _lightModel.Intensity = 255;


            }
            ValidationText = "Run Validation";
         }
         finally
         {
            _validationRoutineRunning = false;
         }


      }
      #endregion

      private void UpdateAllLEDControls()
      {
         // TODO: Possibly a better way would be to have the model notify the ViewModel of changes to properties and then we'd act accordingly (like: _stageModel.PropertyChanged += _stageModel_PropertyChanged;)
         // But do recall, that some some of this stuff such as LEDText has nothing to do with the model. It's our invention for the GUI.

         OnPropertyChanged("LEDSafetyColor");
         OnPropertyChanged("LEDText");
         OnPropertyChanged("LEDFillColor");
         OnPropertyChanged("LEDIntensity");
         OnPropertyChanged("LEDOn");
         OnPropertyChanged("ValidationProcedureEnabled");
      }

      private void UpdateAllStageControls()
      {
         OnPropertyChanged("StageSafetyColor");
         OnPropertyChanged("StagePowerText");
         OnPropertyChanged("SelectedStationDescription");
         OnPropertyChanged("AngleProperty");
         OnPropertyChanged("StagePowerOn");
         OnPropertyChanged("ValidationProcedureEnabled");
      }

      private void _stageModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         Debug.WriteLine("_stageModel_PropertyChanged(), property is: " + e.PropertyName);
         if (e.PropertyName == "Position")  // our stage position has changed.  Update!
         {
            OnPropertyChanged("SelectedStationDescription");
            OnPropertyChanged("AngleProperty");
         }
         else if (e.PropertyName == "On")
         {
            UpdateAllStageControls();
         }
      }

      private void _lightModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         UpdateAllLEDControls();
      }

   }


}
