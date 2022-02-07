using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EighteenFiftyNine.Models.Interfaces
{
   
   public interface IStage : IDevice
   {
      uint Position { get;  set; }
      void Home();
      void RequestStagePositionChange(uint position);
     
   }
}
