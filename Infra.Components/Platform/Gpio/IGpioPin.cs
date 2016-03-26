using System;

namespace Infra.Components.Platform.Gpio
{
    public interface IGpioPin : IDisposable
    {
        void Write(GpioPinValue value);
        GpioPinValue Read();
        void SetDriveMode(GpioPinDriveMode value);
    }
}
