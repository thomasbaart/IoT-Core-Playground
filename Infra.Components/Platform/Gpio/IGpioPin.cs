namespace Infra.Components.Platform.Gpio
{
    public interface IGpioPin
    {
        void Write(GpioPinValue value);
        GpioPinValue Read();
        void SetDriveMode(GpioPinDriveMode value);
    }
}
