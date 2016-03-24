using Infra.Components.Platform.Gpio;

namespace Infra.Components.OutputDevices
{
    public class Led : ILed
    {
        private IGpioController _controller;
        private IGpioPin _pin;

        public Led(IGpioController controller, int pin)
        {
            _controller = controller;
            _pin = _controller.OpenPin(pin);
            _pin.SetDriveMode(GpioPinDriveMode.Output);
        }

        public void Off()
        {
            _pin.Write(GpioPinValue.Low);
        }

        public void On()
        {
            _pin.Write(GpioPinValue.High);
        }
    }
}
