using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EighteenFiftyNine.Models.Interfaces
{
   public interface IDevice : INotifyPropertyChanged
   {
      bool On { get; set; }
      bool Initialize(out string reason);
      bool Shutdown(out string reason);
      
   }
}
