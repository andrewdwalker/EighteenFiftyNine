using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EighteenFiftyNine
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
         var nameOfPropertyInVm = "AngleProperty";
         var binding = new Binding(nameOfPropertyInVm) { Mode = BindingMode.OneWay };
         this.SetBinding(AngleProperty, binding);
      }

      private void PreviewLEDIntensityInput(object sender, TextCompositionEventArgs e)
      {
         e.Handled = !IsLEDValueValue(((TextBox)sender).Text + e.Text);
      }

      public static bool IsLEDValueValue(string str)
      {
         int i;
         return int.TryParse(str, out i) && i >= 0 && i <= 255;
      }

      public double Angle
      {
         get { return (double)GetValue(AngleProperty); }
         set { SetValue(AngleProperty, value); }
      }

      // Using a DependencyProperty as the backing store for Angle.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(MainWindow), new UIPropertyMetadata(0.0, new PropertyChangedCallback(AngleChanged)));

      private static void AngleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
      {
         MainWindow control = (MainWindow)sender;
         control.PerformAnimation((double)e.OldValue, (double)e.NewValue);
      }

      private void PerformAnimation(double oldValue, double newValue)
      {
         // if we are not careful, the animation will take the "long way" round.  It would be nice if there was some 
         // sort of "go counter-clockwise" command, but I don't see it. Thus, we resort to trickery!
         if (newValue - oldValue > 180)
            newValue = newValue - 360;
         else if (newValue - oldValue < -180)
            newValue = 360 + newValue;

         // get a rough estimate of time that corresponds to the idea of 1 second between stations. Recall there are 11 stations (including home!) in a circle of 360 degrees. Simple math.
         double seconds = Math.Abs((newValue - oldValue) * 11.0 / 360.0);
         Debug.WriteLine("Seconds in PerformAnimation: " + seconds);
         var rotationAnimation = new DoubleAnimation(oldValue, newValue, TimeSpan.FromSeconds(seconds));
         rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);

      }

   }
}
