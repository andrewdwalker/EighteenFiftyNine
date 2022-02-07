using EighteenFiftyNine.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EighteenFiftyNine.Models.Classes
{
   public enum StageDirection
   {
      Clockwise = 0,
      CounterClockwise =1,
   }
   public class Stage : Device, IStage
   {
      private uint _position = 0;  // 0 is HOME
      private bool _on = false;
      public uint Position
      {
         get => _position;
         set
         {
            int x = -12 % 11;
            int y = -1 % 11;
            int z = 12 % 11;
            if (value < 0 || value > 10)
            {
               throw new ArgumentOutOfRangeException("Position of Stage must be between 0(HOME) and 10 inclusive");
            }

            // We will assume motors are capable of going both directions and therefore pick shortest route to introduce delay
            // TODO. Consider something slicker than a simple Sleep which will lock GUI and not allow for cancellation
            int delay = 0;
            if (value != _position)
            {
               // introduce some delay. Presumeably this would be at hardware level!
               delay = 1;// Math.Abs((int)value -(int) _position); Take into account delays more than 1 (offset stations) later!
            }

            _position = value;
            OnPropertyChanged("Position");

            // TODO REVISIT THIS! We are living dangerously. We are telling the VM now about our move, so the GUI can start updating, but of course, in the real world, we don't
            // know yet that the move has been successful. A better implementation might be a "start of move" and "end of move" property!
            Thread.Sleep(delay * 1000);

           
           
         }


      }
      public bool On {
         get => _on; 
         set
         {
            _on = value;
            OnPropertyChanged("On");
         }
      }

      

      public void Home()
      {
         RequestStagePositionChange(0);
      }

      public bool Initialize(out string reason)
      {
         reason = "OK";
         On = true;
         Home();
         return true;
      }

      /// <summary>
      /// Note: A nice explanation of this travelling around a circle ..
      /// https://stackoverflow.com/questions/9505862/shortest-distance-between-two-degree-marks-on-a-circle
      /// </summary>
      /// <param name="position"></param>
      public void RequestStagePositionChange(uint position)
      {
         int difference = ((int) position - (int) _position) % 11;
         if (difference > 5)
         {
            difference = difference - 11;
         }
         else if (difference < -5)
         {
            difference = difference + 11;
         }
         for (int i = 0; i < Math.Abs(difference); i++)
         {
            
            MoveStage(difference > 0 ? 1 : -1);
         }
        

         
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="numberOfUnits Positive if clockwise, else counter clockwise"></param>
      private void MoveStage(int numberOfUnits)
      {
         int newPosition = ((int)_position + numberOfUnits) % 11;
         if (newPosition < 0)
            newPosition = 11 + newPosition;
         Position = (uint)newPosition;
      }

      public bool Shutdown(out string reason)
      {
         reason = "OK";  // TODO probably reason should be enums or defines or similar
         // Presumeably, a shutdown should be graceful and leave the device is a known state. Thus Home and Power down.
         // do we have power?
         if (!On)
         {
            // TODO Log and take other appropriate actions
            reason = "No Power";
            return false;
         }
         else
         {
            Home();
            On = false;
            return true;
         }
      }

      
   }
}
