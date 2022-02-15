using EighteenFiftyNine.Models.Classes;
using EighteenFiftyNine.Models.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModelUnitTests
{
   [TestClass]
   public class ModelUnitTests
   {
     
      /// <summary>
      /// Test basic functioning of Property "Position"
      /// </summary>
      [TestMethod]
      public async Task RequestStagePositionChangeTest1()
      {
         IStage st = new Stage(new HardwareStage());
         st.On = true;
         Assert.AreEqual((uint) 0, st.Position);  // Position at start up is 0.
         var result = await st.RequestStagePositionChange(5);
         Assert.AreEqual((uint) 5, st.Position);
         Assert.AreEqual(result.Item1, true);
         st.Dispose();
         
      }

      /// <summary>
      /// Test setting position without turning on power. Should fail
      /// </summary>
      /// <returns></returns>
      [TestMethod]
      public async Task RequestStagePositionChangeTest2()
      {
         IStage st = new Stage(new HardwareStage());
        
         Assert.AreEqual((uint)0, st.Position);  // Position at start up is 0.
         var result = await st.RequestStagePositionChange(5);
         Assert.AreEqual(result.Item1, false);
         Assert.AreNotEqual((uint)5, st.Position);
         Assert.AreEqual((uint)0, st.Position); // since power was off, position should still be 0.
         st.Dispose();
      }

      /// <summary>
      /// Use a faulty stage. Mocking would be another idea here.
      /// Test neither setting stage position OR retrieving stage position is correct
      /// NOTICE use of using, which in this case is very necessary. We are throwing an exception, so st.Dispose won't be called. But "using" guarantees Dispose will be called.
      /// </summary>
      /// <returns></returns>
      [TestMethod]
      [ExpectedException(typeof(HardwareException))]
      public async Task RequestStagePositionChangeTest3()
      {
         using (var faultyHardwareStage = new FaultyHardwareStage(new List<byte>() { (byte)CommandBytes.MoveStage, (byte)CommandBytes.QueryStagePosition }))
         {
            IStage st = new Stage(faultyHardwareStage);
            st.On = true;
            Assert.AreEqual((uint)0, st.Position);  // Position at start up is 0.
            var result = await st.RequestStagePositionChange(5);
            Assert.AreEqual(result.Item1, false);
            Assert.AreNotEqual((uint)5, st.Position);
            //st.Dispose();  // won't be called.
         }
        
      }

      /// <summary>
      /// Use a faulty stage. Mocking would be another idea here.
      /// Test  setting stage position fails, but reading works
      /// </summary>
      /// <returns></returns>
      [TestMethod]
      public async Task RequestStagePositionChangeTest4()
      {
         IStage st = new Stage(new FaultyHardwareStage(new List<byte>() { (byte) CommandBytes.MoveStage }));
         st.On = true;
         Assert.AreEqual((uint)0, st.Position);  // Position at start up is 0.
         var result = await st.RequestStagePositionChange(5);
         Assert.AreEqual(result.Item1, false);
         Assert.AreNotEqual((uint)5, st.Position);
         Assert.AreEqual((uint)0, st.Position); // since we didn't move, still at position 0. Of course, we could fail in other ways, which we should also write unit test for!
         st.Dispose();

      }

      [TestMethod]
      public void OnTest1()
      {

         IStage st = new Stage(new HardwareStage());
         Assert.AreEqual(st.On, false);
         st.On = true;
         Assert.AreEqual(st.On, true);  
         
         st.Dispose();

      }

      [TestMethod]
      [ExpectedException(typeof(HardwareException))]
      public void OnTest2()
      {
         using (var faultyHardwareStage = new FaultyHardwareStage(new List<byte>() { (byte)CommandBytes.QueryOnOff }))
         {
            IStage st = new Stage(faultyHardwareStage);
            bool thisShouldCrash = st.On;
         }
      }

      [TestMethod]
      public async Task HomeTest1()
      {

         IStage st = new Stage(new HardwareStage());       
         st.On = true;
         await st.RequestStagePositionChange(7); // move stage away from HOME
         Assert.AreEqual((uint)7, st.Position);
         await st.Home();
         Assert.AreEqual((uint)0, st.Position);

         st.Dispose();

      }

   }
}
