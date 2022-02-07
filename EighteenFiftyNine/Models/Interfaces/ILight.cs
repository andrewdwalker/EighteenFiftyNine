using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EighteenFiftyNine.Models.Interfaces
{
   
   public interface ILight : IDevice
   {
      
      byte Intensity { get; set; }
      
   }
}
