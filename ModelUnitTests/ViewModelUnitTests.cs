using EighteenFiftyNine.Models.Classes;
using EighteenFiftyNine.Models.Interfaces;
using EighteenFiftyNine.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ViewModelUnitTests
{
   /// <summary>
   /// Summary description for ViewModelUnitTests
   /// </summary>
   [TestClass]
   public class ViewModelUnitTests
   {

      /// <summary>
      /// Simple test of construction
      /// </summary>
      [TestMethod]
      public void ConstructorTest()
      {
         IStage stage = new Stage(new HardwareStage());
         var mock = new Mock<ILight>();
         EighteenFiftyNineViewModel vm = new EighteenFiftyNineViewModel(stage, mock.Object);

         Assert.IsTrue(vm.StationDescriptions.Count == 11);
         stage.Dispose();
      }


      /// <summary>
      /// Check results of Light on property...
      /// uses mocks.
      /// </summary>
      [TestMethod]
      public void LightOnTest1()
      {
         var mockLight = new Mock<ILight>();
         mockLight.SetupGet(x => x.On).Returns(true);
         var mockStage = new Mock<IStage>();

         EighteenFiftyNineViewModel vm = new EighteenFiftyNineViewModel(mockStage.Object, mockLight.Object);
         Assert.AreEqual(vm.LEDOn, true);
         Assert.AreEqual(vm.LEDText, "LED Power is On!");
      }

      /// <summary>
      /// Check SolidColorBrush is okay
      /// uses mocks.
      /// </summary>
      [TestMethod]
      public void LightOnTest2()
      {
         var mockLight = new Mock<ILight>();
         mockLight.SetupGet(x => x.On).Returns(true);
         mockLight.SetupGet(x => x.Intensity).Returns(100);
         var mockStage = new Mock<IStage>();

         Color clrExpected = Colors.Green;
         clrExpected.A = 100;
         EighteenFiftyNineViewModel vm = new EighteenFiftyNineViewModel(mockStage.Object, mockLight.Object);
         SolidColorBrush clr = vm.LEDFillColor;
         Assert.IsTrue(clr.Color == clrExpected);
         Assert.IsTrue(clr.Color.A == 100);

      }

      /// <summary>
      /// We expect the stage to be at position 10 and the LED to be off at end of validation.
      /// </summary>
      [TestMethod]
      public async Task ValidationCommandImplementationTest()
      {
         IStage stage = new Stage(new HardwareStage());
         ILight light = new Light();
         EighteenFiftyNineViewModel vm = new EighteenFiftyNineViewModel(stage, light);
         vm.ToggleStagePower.Execute(null);
         vm.ToggleLED.Execute(null);

         // Power should now be on for stage and LED, and we can run Validation Procedure
         Assert.IsTrue(vm.StagePowerOn);
         Assert.IsTrue(vm.LEDOn);

         await vm.ValidationCommandImplementation();



         // We now expect LED to be off and stage to be at postion 10
         Assert.IsFalse(vm.LEDOn);
         Assert.IsTrue(vm.SelectedStationDescription.StationNumber == 10);

         stage.Dispose();
      }
   }
}
