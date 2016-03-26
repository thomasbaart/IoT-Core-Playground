using Infra.Components.Platform.Common;
using System;

namespace Infra.Components.OutputDevices
{
    public interface ILed : IDisposable
    {
        DeviceState State { get; }
        void On();
        void Off();
    }
}
