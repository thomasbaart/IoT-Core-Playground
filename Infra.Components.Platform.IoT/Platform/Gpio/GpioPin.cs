using Infra.Components.Platform.Gpio;

namespace Infra.Components.Platform.IoT.Platform.Gpio
{
    public class GpioPin : IGpioPin
    {
        private Windows.Devices.Gpio.GpioPin _pin;

        public GpioPin(Windows.Devices.Gpio.GpioPin pin)
        {
            _pin = pin;
        }

        public GpioPinValue Read()
        {
            return _pin.Read().ToInfraGpioPinValue();
        }

        public void SetDriveMode(GpioPinDriveMode value)
        {
            _pin.SetDriveMode(value.ToWindowsGpioPinDriveMode());
        }

        public void Write(GpioPinValue value)
        {
            _pin.Write(value.ToWindowsGpioPinValue());
        }

        public void Dispose()
        {
            _pin.Dispose();
        }
    }
}
