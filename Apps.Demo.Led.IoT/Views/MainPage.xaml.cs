using Infra.Components.OutputDevices;
using Infra.Components.Platform.IoT.Platform.Unity;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Apps.Demo.Blink.IoT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            IUnityContainer container = new UnityContainer();
            PlatformConfiguration.ConfigureContainer(container);
            ComponentConfiguration.ConfigureContainer(container);
            ILed led = container.Resolve<ILed>(new ParameterOverride("pin", 5));

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => {
                switch (led.State)
                {
                    case Infra.Components.Platform.Common.DeviceState.Off:
                        led.On();
                        break;
                    case Infra.Components.Platform.Common.DeviceState.On:
                        led.Off();
                        break;
                    default:
                        break;
                }
            };
            timer.Start();
        }
    }
}
