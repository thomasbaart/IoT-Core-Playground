using Infra.Components.Platform.Gpio;
using System;

namespace Infra.Components.Platform.Common
{
    public static class DeviceStateExtensions
    {
        public static GpioPinValue ToGpioPinValue(this DeviceState deviceState)
        {
            GpioPinValue pinValue;

            switch (deviceState)
            {
                case DeviceState.On:
                    pinValue = GpioPinValue.High;
                    break;
                case DeviceState.Off:
                    pinValue = GpioPinValue.Low;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("deviceState");
            }

            return pinValue;
        }
    }
}
