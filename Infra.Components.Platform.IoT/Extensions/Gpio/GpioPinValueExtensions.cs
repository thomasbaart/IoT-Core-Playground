using Infra.Components.Platform.Gpio;
using System;

namespace Infra.Components.Platform.IoT.Extensions.Gpio
{
    public static class GpioPinValueExtensions
    {
        public static Windows.Devices.Gpio.GpioPinValue ToWindowsGpioPinValue(this GpioPinValue pinValue)
        {
            Windows.Devices.Gpio.GpioPinValue windowsPinValue = Windows.Devices.Gpio.GpioPinValue.Low;

            switch (pinValue)
            {
                case GpioPinValue.Low:
                    windowsPinValue = Windows.Devices.Gpio.GpioPinValue.Low;
                    break;
                case GpioPinValue.High:
                    windowsPinValue = Windows.Devices.Gpio.GpioPinValue.High;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("pinValue");
            }

            return windowsPinValue;
        }

        public static GpioPinValue ToInfraGpioPinValue(this Windows.Devices.Gpio.GpioPinValue pinValue)
        {
            GpioPinValue infraPinValue = GpioPinValue.Low;

            switch (pinValue)
            {
                case Windows.Devices.Gpio.GpioPinValue.Low:
                    infraPinValue = GpioPinValue.Low;
                    break;
                case Windows.Devices.Gpio.GpioPinValue.High:
                    infraPinValue = GpioPinValue.High;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("pinValue");
            }

            return infraPinValue;
        }
    }
}
