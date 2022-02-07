using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EighteenFiftyNine.Models.Classes
{
   public class Device
   {
      public event PropertyChangedEventHandler PropertyChanged;
      protected virtual void OnPropertyChanged(string propertyName)
      {
         //this.VerifyPropertyName(propertyName);

         PropertyChangedEventHandler handler = this.PropertyChanged;
         if (handler != null)
         {
            var e = new PropertyChangedEventArgs(propertyName);
            handler(this, e);
         }
      }
   }
}
