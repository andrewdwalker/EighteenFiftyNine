using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EighteenFiftyNine.Models.Classes
{
   // A class for testing. Hardware stage that allows us to not respond to certain messages.
   // Gives a very concrete example of why it's useful to deal with IStage rather than Stage
   public class FaultyHardwareStage : HardwareStage
   {
      private List<byte> _badMessages = null;
      public FaultyHardwareStage(List<byte> badMessages) : base()
      {
         _badMessages = badMessages;
      }
      protected override void ParseMsg(NetworkStream stream, byte[] message)
      {
         // refuse to ack any messages that are in our list of badMessages.  
         if (_badMessages != null && _badMessages.Contains(message[0]))
         {
            stream.Write(NACKBYTES, 0, ACKBYTES.Length);
           return;
         }
         switch (message[0])
         {
            case 0:
               _powerOn = 0;
               stream.Write(ACKBYTES, 0, ACKBYTES.Length);
               break;
            case 1:
               _powerOn = 1;
               stream.Write(ACKBYTES, 0, ACKBYTES.Length);
               break;
            case 3:
               if (message[1] <= 10 && message[1] >= 0 && _powerOn == 1)
               {

                  // take 1 second for each move between stations, as per requirements. Simulate hardware slowness. A better architecture for the hardware would probably be to acknowledge immediately and then send follow up info or wait until asked.
                  if (_stagePosition != message[1])
                  {

                     int delay = Math.Abs(_stagePosition - message[1]);
                     delay = (delay < 5) ? delay : 11 - delay;
                     Thread.Sleep(delay * 1000);
                  }

                  _stagePosition = message[1];
                  stream.Write(ACKBYTES, 0, ACKBYTES.Length);
               }
               else
               {
                  stream.Write(NACKBYTES, 0, ACKBYTES.Length);
               }
               break;
            case 4:
               stream.Write(ACKBYTES.Concat(new byte[] { _powerOn }).ToArray(), 0, 4);
               break;
            case 5:
               stream.Write(ACKBYTES.Concat(new byte[] { _stagePosition }).ToArray(), 0, 4);
               break;
            default:
               // we couldn't figure out message
               stream.Write(NACKBYTES, 0, ACKBYTES.Length);
               break;
         }
      }
   }
}
