using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Infra.Components.Platform.Gpio;
using Infra.Components.OutputDevices;

namespace Infra.Components.UnitTests.OutputDevices
{
    [TestClass]
    public class LedTests
    {
        private Mock<IGpioController> _controllerMock;
        private Mock<IGpioPin> _pinMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _controllerMock = new Mock<IGpioController>(MockBehavior.Strict);
            _pinMock = new Mock<IGpioPin>(MockBehavior.Strict);

            _controllerMock
                .Setup(c => c.OpenPin(It.Is<int>(p => p == 1)))
                .Returns(_pinMock.Object);

            _pinMock
                .Setup(p => p.SetDriveMode(
                    It.Is<GpioPinDriveMode>(dm => dm == GpioPinDriveMode.Output)));
        }

        [TestMethod]
        public void Led_Ctor_InitializesPinToOutput()
        {
            // Arrange
            // Act
            ILed led = new Led(_controllerMock.Object, 1);

            // Assert
            _controllerMock.VerifyAll();
            _pinMock.VerifyAll();
        }

        [TestMethod]
        public void Led_On_WritesHighValue()
        {
            // Arrange
            _pinMock
                .Setup(p => p.Write(It.Is<GpioPinValue>(pv => pv == GpioPinValue.High)));

            ILed led = new Led(_controllerMock.Object, 1);

            // Act
            led.On();

            // Assert
            _controllerMock.VerifyAll();
            _pinMock.VerifyAll();
        }

        [TestMethod]
        public void Led_Off_WritesLowValue()
        {
            // Arrange
            _pinMock
                .Setup(p => p.Write(It.Is<GpioPinValue>(pv => pv == GpioPinValue.Low)));

            ILed led = new Led(_controllerMock.Object, 1);

            // Act
            led.Off();

            // Assert
            _controllerMock.VerifyAll();
            _pinMock.VerifyAll();
        }
    }
}
