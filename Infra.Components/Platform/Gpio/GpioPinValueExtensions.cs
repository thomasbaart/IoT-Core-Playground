using Infra.Components.Platform.Common;
using System;

namespace Infra.Components.Platform.Gpio
{
    public static class GpioPinValueExtensions
    {
        public static DeviceState ToDeviceState(this GpioPinValue pinValue)
        {
            DeviceState deviceState;

            switch (pinValue)
            {
                case GpioPinValue.Low:
                    deviceState = DeviceState.Off;
                    break;
                case GpioPinValue.High:
                    deviceState = DeviceState.On;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("pinValue");
            }

            return deviceState;
        }
    }
}
