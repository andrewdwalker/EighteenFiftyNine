using EighteenFiftyNine.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EighteenFiftyNine.Models.Classes
{

   public class Light : Device, ILight
   {
      private bool _on = false;
      private byte _intensity = 50;

     
      public bool On
      {
         get => _on;
         set
         {
            _on = value;
            OnPropertyChanged("On");
         }
      }

      public byte Intensity
      {
         get => _intensity;
         set 
            { 
            _intensity = value;
            OnPropertyChanged("Intensity");
         }
      }

      public bool Initialize(out string reason)
      {
         throw new NotImplementedException();
      }

      public bool Shutdown(out string reason)
      {
         reason = "OK";
         Intensity = 0;
         On = false;
         return true;
      }
   }
}
