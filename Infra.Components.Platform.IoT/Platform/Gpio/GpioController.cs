using Infra.Components.Platform.Gpio;

namespace Infra.Components.Platform.IoT.Platform.Gpio
{
    public class GpioController : IGpioController
    {
        private Windows.Devices.Gpio.GpioController _controller;

        public GpioController(Windows.Devices.Gpio.GpioController controller)
        {
            _controller = controller;
        }

        public IGpioPin OpenPin(int pinNumber)
        {
            return new GpioPin(_controller.OpenPin(pinNumber));
        }
    }
}
