using Infra.Components.Platform.Gpio;
using Infra.Components.Platform.Common;

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
            _pin.Write(GpioPinValue.Low);
        }

        public DeviceState State
        {
            get
            {
                return _pin.Read().ToDeviceState();
            }
        }

        public void Dispose()
        {
            _pin.Dispose();
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
