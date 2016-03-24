using Infra.Components.OutputDevices;
using Microsoft.Practices.Unity;

namespace Infra.Components.Platform.IoT.Platform.Unity
{
    public class ComponentConfiguration
    {
        public static void ConfigureContainer(IUnityContainer container)
        {
            container.RegisterType(typeof(ILed), typeof(Led));
        }
    }
}
