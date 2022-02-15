
using EighteenFiftyNine.Models.Classes;
using EighteenFiftyNine.Models.Interfaces;
using EighteenFiftyNine.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EighteenFiftyNine
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      protected override void OnStartup(StartupEventArgs e)
      {
         MainWindow view = new MainWindow();
         IStage stageModel = new Stage(new HardwareStage());
         ILight lightModel = new Light();
         EighteenFiftyNineViewModel viewModel = new EighteenFiftyNineViewModel(stageModel, lightModel);
         view.DataContext = viewModel;
         view.Show();

      }
   }
}
