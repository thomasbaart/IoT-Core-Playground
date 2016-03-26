using Infra.Components.Platform.Gpio;
using System;

namespace Infra.Components.Platform.IoT.Platform.Gpio
{
    public static class GpioPinDriveModeExtensions
    {
        public static Windows.Devices.Gpio.GpioPinDriveMode ToWindowsGpioPinDriveMode(this GpioPinDriveMode driveMode)
        {
            Windows.Devices.Gpio.GpioPinDriveMode windowsDriveMode = Windows.Devices.Gpio.GpioPinDriveMode.Input;

            switch (driveMode)
            {
                case GpioPinDriveMode.Input:
                    windowsDriveMode = Windows.Devices.Gpio.GpioPinDriveMode.Input;
                    break;
                case GpioPinDriveMode.Output:
                    windowsDriveMode = Windows.Devices.Gpio.GpioPinDriveMode.Output;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("driveMode");
            }

            return windowsDriveMode;
        }

        public static GpioPinDriveMode ToInfraGpioPinDriveMode(this Windows.Devices.Gpio.GpioPinDriveMode driveMode)
        {
            GpioPinDriveMode infraDriveMode = GpioPinDriveMode.Input;

            switch (driveMode)
            {
                case Windows.Devices.Gpio.GpioPinDriveMode.Input:
                    infraDriveMode = GpioPinDriveMode.Input;
                    break;
                case Windows.Devices.Gpio.GpioPinDriveMode.Output:
                    infraDriveMode = GpioPinDriveMode.Output;
                    break;
                case Windows.Devices.Gpio.GpioPinDriveMode.InputPullUp:
                    throw new NotImplementedException();
                case Windows.Devices.Gpio.GpioPinDriveMode.InputPullDown:
                    throw new NotImplementedException();
                case Windows.Devices.Gpio.GpioPinDriveMode.OutputOpenDrain:
                    throw new NotImplementedException();
                case Windows.Devices.Gpio.GpioPinDriveMode.OutputOpenDrainPullUp:
                    throw new NotImplementedException();
                case Windows.Devices.Gpio.GpioPinDriveMode.OutputOpenSource:
                    throw new NotImplementedException();
                case Windows.Devices.Gpio.GpioPinDriveMode.OutputOpenSourcePullDown:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("driveMode");
            }

            return infraDriveMode;
        }
    }
}
