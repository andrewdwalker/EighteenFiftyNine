using EighteenFiftyNine.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EighteenFiftyNine.Models.Classes
{

   public enum CommandBytes : byte
   {
      PowerOff = 0,
      PowerOn = 1,
      MoveStage = 3, // second byte will give which station to move to with 0 being HOME, or 1-10 inclusive
      QueryOnOff = 4, // return 1 (On) or 2 (Off)
      QueryStagePosition = 5, // return 0-10 OR NACK if it command doesn't make sense or position can't be determined.

   }
   /// <summary>
   /// Simple Class to represent hardware stage.
   /// This casually represents hardware so I'm not going to spend much time on it. Notice the TCPListener is always on for instance. And no unit tests.
   /// This is a standin for hardware that's (hopefully) coming
   /// Also note it's quite dumb and far from ideal. We make requests and have to wait for the answer. There is no "incremental progress". 
   /// See above for the very simple messaging protocal based on the first 1 byte (2 bytes in case of moving stage to a given position)
   /// </summary>
   public class HardwareStage : IHardwareStage
   {
      // Set the TcpListener on port 13000.
      private readonly int _Port = 13000;
      IPAddress _localAddr = IPAddress.Parse("127.0.0.1");  // probably would not hardcode these in real implentation :)
      private TcpListener _server = null;
      protected byte _powerOn = 0;
      protected byte _stagePosition = 0;
      protected byte[] ACKBYTES = Encoding.ASCII.GetBytes("ACK");
      protected byte[] NACKBYTES = Encoding.ASCII.GetBytes("NACK");

      private Task _hardwareStageTask;

      public HardwareStage()
      {
         // Just set everything up in construct. Don't worry now about errors in setting up hardware or breaking out into separate methods. This is just fake hardware.
         
         CommLoop();
      }

     

      /// <summary>
      /// Very much based on:
      /// https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener?view=net-6.0
      /// </summary>
      /// <returns></returns>
      private void CommLoop()
      {

         // TcpListener server = new TcpListener(port);
         _server = new TcpListener(_localAddr, _Port);

         // Start listening for client requests.
         _server.Start();

         // Buffer for reading data
         Byte[] bytes = new Byte[256];
         String data = null;

         // Enter the listening loop.
         _hardwareStageTask = Task.Run(() =>
         {
            while (true)
            {
               Console.Write("Waiting for a connection... ");

               // Perform a blocking call to accept requests.
               // You could also use server.AcceptSocket() here.
               TcpClient client = _server.AcceptTcpClient();
               Console.WriteLine("Connected!");

               data = null;

               // Get a stream object for reading and writing
               NetworkStream stream = client.GetStream();

               int i;


               // Loop to receive all the data sent by the client.
               while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
               {
                  // Translate data bytes to a ASCII string.
                  data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                  Console.WriteLine("Received: {0}", data);

                  // Process the data sent by the client.
                  data = data.ToUpper();

                  byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                  ParseMsg(stream, msg);

                 
               }

               // Shutdown and end connection
               client.Close();
            }
         });
      }


      protected virtual void ParseMsg(NetworkStream stream, byte[] message)
      {
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

      /// <summary>
      /// I originally didn't have the Dispose in here. Unfortunately, the Unit Tests break if you don't have this.
      /// It's not necessary for running the main program. The problem is the socket connection must be stopped if you 
      /// plan on using it again quickly (like in Unit Tests)
      /// </summary>
      public void Dispose()
      {
         Dispose(true);

      }

      private bool _disposedValue;
      protected virtual void Dispose(bool disposing)
      {
         if (!_disposedValue)
         {
            if (disposing)
            {
               _server.Stop();
               _server = null;
            }

            _disposedValue = true;
         }
        

      }
   }
}
