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
         EighteenFiftyNineViewModel viewModel = new EighteenFiftyNineViewModel();
         view.DataContext = viewModel;
         view.Show();

      }
   }
}
