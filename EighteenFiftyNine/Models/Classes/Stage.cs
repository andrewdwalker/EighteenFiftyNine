using EighteenFiftyNine.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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

   public class HardwareException : Exception
   {
      public HardwareException() : base() { }
      public HardwareException(string message) : base(message) { }
   }
   public class Stage : Device, IStage
   {
      private uint _position = 0;  // 0 is HOME. Presumeably in a real system, we'd go and find out the actual position.
      private IHardwareStage _hardwareStage;

      TcpClient _client = null;
      NetworkStream _stream = null;

      public Stage(IHardwareStage hardwareStage)
      {
         _hardwareStage = hardwareStage;
      }

      public uint Position
      {
         get
         {
            return _position;          
         }
      }
      
      public bool On {
         get
         {
            string answer = WriteToHardware((byte)CommandBytes.QueryOnOff);
            if (!answer.ToUpper().Contains("ACK"))
               throw new HardwareException();  // We should probably get a little fancier and see if it's a "NACK" or something else and if there is addtional info in error from hardware
            else
            {
               // 4th byte shows position
               return System.Text.Encoding.ASCII.GetBytes(answer)[3] == 1 ? true : false;
            }
         }
         set
         {
           
            string answer = WriteToHardware(value ? (byte)1 : (byte) 0);
            if (!answer.ToUpper().Contains("ACK"))
               throw new HardwareException();  // We should probably get a little fancier and see if it's a "NACK" or something else and if there is addtional info in error from hardware
            else
               OnPropertyChanged("On");
         }
      }

      

      public async Task<(bool, string)> Home()
      {
         return await RequestStagePositionChange(0);
      }

      public async Task<(bool, string)> Initialize()  // Currently not used. I have logic for Initialize in VM. Should fix that....
      {
         
         On = true;
         return await Home();
         
      }

      /// <summary>
      /// Note: A nice explanation of this travelling around a circle ..
      /// https://stackoverflow.com/questions/9505862/shortest-distance-between-two-degree-marks-on-a-circle
      /// </summary>
      /// <param name="position"></param>
      public async Task<(bool, string)> RequestStagePositionChange(uint position)
      {
         _position = position;
         OnPropertyChanged("Position"); // we initial tell ViewModel that we have reached position so VM can start animation. We will correct later.
         string answer = await WriteToHardwareAsync(new byte[] { 3, (byte) position });
         
         if (!answer.ToUpper().Contains("ACK"))
         {
            // we didn't set position like we thought. That means our animation is wrong. Get the current position and send it along
            // AND return false, not true!
             answer = WriteToHardware(new byte[] { (byte)CommandBytes.QueryStagePosition });
            if (!answer.ToUpper().Contains("ACK"))  // we can't even read back the position!  Faulty hardware? This is unexpected so throw an exception and let ViewModel deal with it!
               throw new HardwareException("Could not communicate with hardware");  // We should probably get a little fancier and see if it's a "NACK" or something else and if there is addtional info in error from hardware
            else
            {
               // 4th byte shows position
               _position = (uint)System.Text.Encoding.ASCII.GetBytes(answer)[3];
               OnPropertyChanged("Position"); // make sure Position is corrected for VM
            }
            return (false, answer);
            
         }
         else
         {
            // Okay, looks like we set position correctly. Just verify and update. Position won't change from our set position, so we're firing a property for no reason. That's probably okay!
            answer = WriteToHardware(new byte[] { (byte)CommandBytes.QueryStagePosition });
            if (!answer.ToUpper().Contains("ACK"))
               throw new HardwareException();  // We should probably get a little fancier and see if it's a "NACK" or something else and if there is addtional info in error from hardware
            else
            {
               // 4th byte shows position
               _position = (uint)System.Text.Encoding.ASCII.GetBytes(answer)[3];
               OnPropertyChanged("Position"); // Consider whether we can get rid of this entire else block!

               return (true, answer);  
            }
         }
        

      }

      
      public async Task<(bool,string)> Shutdown()
      {
        
         // Presumeably, a shutdown should be graceful and leave the device is a known state. Thus Home and Power down.
         // do we have power?
         if (!On)
         {
            // TODO Log and take other appropriate actions
           
            return (false, "No Power"); ;
         }
         else
         {
            await Home();
            On = false;
            return (true, "Shutdown");
         }
      }

      void CleanupSocket()
      {
         // Close everything.
         if (_stream != null)
         {
            _stream.Close();
            _stream = null;
         }
         if (_client != null)
         {
            _client.Close();
            _client = null;
         }
      }
      /// <summary>
      /// Very much based on: https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-6.0
      /// </summary>
      /// <param name="message"></param>
      private async Task<string> WriteToHardwareAsync(byte[] data)
      {
         //TcpClient _client = null;
         //NetworkStream _stream = null;
         try
         {
            CleanupSocket();
            string server = "127.0.0.1"; // hardcoded for now

            Int32 port = 13000; // hardcoded for now
            _client = new TcpClient(server, port);

            _stream = _client.GetStream();

            _stream.ReadTimeout = _stream.WriteTimeout = 15000; // don't wait forever
            // Send the message to the connected TcpServer.
            await _stream.WriteAsync(data, 0, data.Length);



            // Receive the TcpServer.response.

            // Buffer to store the response bytes.
            data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = await _stream.ReadAsync(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Debug.WriteLine("Received: {0}", responseData);

            // Close everything.
            CleanupSocket();
            return responseData;
         }

         catch (SocketException e)
         {
            Debug.WriteLine("SocketException: {0}", e); // Also log etc.
            return string.Format("SocketException: {0}", e);
         }
         catch (IOException e)
         {
            Debug.WriteLine("IOException: {0}", e); // Also log etc.
            return string.Format("IOException: {0}", e);
         }
         catch (Exception e)
         {
            Debug.WriteLine("Exception: {0}", e); // Also log. ESPECIALLY important as this is something we really weren't expecting.
            return string.Format("Exception: {0}", e);
         }
         finally
         {
            // Close everything.
            // CleanupSocket
         }


      }

      

      /// <summary>
      /// Convenience function
      /// </summary>
      /// <param name="datum"></param>
      /// <returns></returns>
      private async Task<string> WriteToHardwareAsync(byte datum)
      {
         return await WriteToHardwareAsync(new byte[] { datum });
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="data"></param>
      /// <returns></returns>
      private string WriteToHardware(byte[] data)
      {
         //TcpClient _client = null;
         //NetworkStream _stream = null;
         try
         {
            CleanupSocket();
            string server = "127.0.0.1"; // hardcoded for now

            Int32 port = 13000; // hardcoded for now
            _client = new TcpClient(server, port);

            _stream = _client.GetStream();
            _stream.ReadTimeout = _stream.WriteTimeout = 5000; // don't wait forever

            // Send the message to the connected TcpServer.
           
            _stream.Write(data, 0, data.Length);



            // Receive the TcpServer.response.

            // Buffer to store the response bytes.
            data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
           
            Int32 bytes = _stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Debug.WriteLine("Received: {0}", responseData);

            CleanupSocket();
            return responseData;
         }

         catch (SocketException e)
         {
            Debug.WriteLine("SocketException: {0}", e); // Also log etc.
            return string.Format("SocketException: {0}", e);
         }
         catch(IOException e)
         {
            Debug.WriteLine("IOException: {0}", e); // Also log etc.
            return string.Format("IOException: {0}", e);
         }
         catch (Exception e)
         {
            Debug.WriteLine("Exception: {0}", e); // Also log. ESPECIALLY important as this is something we really weren't expecting.
            return string.Format("Exception: {0}", e);
         }
         finally
         {
            // Close everything.
            // CleanupSocket
         }

      }

      /// <summary>
      /// Convenience function
      /// </summary>
      /// <param name="datum"></param>
      /// <returns></returns>
      private string WriteToHardware(byte datum)
      {
         return WriteToHardware(new byte[] { datum });
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      private bool _disposedValue;
      protected virtual void Dispose(bool disposing)
      {
         if (!_disposedValue)
         {
            if (disposing)
            {
               _hardwareStage.Dispose();
            }

            _disposedValue = true;
         }
      }
   }
}
