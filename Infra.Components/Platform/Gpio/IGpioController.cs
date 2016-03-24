namespace Infra.Components.Platform.Gpio
{
    public interface IGpioController
    {
        IGpioPin OpenPin(int pinNumber);
    }
}
