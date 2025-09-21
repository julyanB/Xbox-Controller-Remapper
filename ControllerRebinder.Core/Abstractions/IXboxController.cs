using ControllerRebinder.Core.Models;

namespace ControllerRebinder.Core.Abstractions;

public interface IXboxController
{
    bool IsConnected { get; }

    GamepadSnapshot GetState();
}
