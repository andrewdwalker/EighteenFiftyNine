using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EighteenFiftyNine.Models.Interfaces
{
   
   public interface IStage : IDevice, IDisposable
   {
      uint Position { get;   }
      Task<(bool,string)> Home();
      Task<(bool, string)> RequestStagePositionChange(uint position);
      Task<(bool,string)> Initialize();
      Task<(bool,string)> Shutdown();

   }
}
