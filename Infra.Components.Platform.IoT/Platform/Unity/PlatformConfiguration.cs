using Infra.Components.Platform.Gpio;
using Infra.Components.Platform.IoT.Platform.Gpio;
using Microsoft.Practices.Unity;

namespace Infra.Components.Platform.IoT.Platform.Unity
{
    public class PlatformConfiguration
    {
        public static void ConfigureContainer(IUnityContainer container)
        {
            container.RegisterType(typeof(IGpioController), typeof(GpioController));
            container.RegisterInstance(typeof(Windows.Devices.Gpio.GpioController),
                Windows.Devices.Gpio.GpioController.GetDefault());
        }
    }
}
