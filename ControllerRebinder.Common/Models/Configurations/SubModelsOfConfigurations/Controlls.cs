using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace ControllerRebinder.Common.Models.Configurations.SubModelsOfConfigurations;

public class Controlls
{
    [JsonConverter(typeof(StringEnumConverter))]
    public VirtualKeyCode Up { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public VirtualKeyCode Down { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public VirtualKeyCode Left { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public VirtualKeyCode Right { get; set; }

    public async Task ReleaseAll_v03(InputSimulator _inputSimulator)
    {
        await Task.Run(() =>
        {
            _inputSimulator.Keyboard.KeyUp(Up);
            _inputSimulator.Keyboard.KeyUp(Down);
            _inputSimulator.Keyboard.KeyUp(Left);
            _inputSimulator.Keyboard.KeyUp(Right);
        });

    }

    public void ReleaseAll_v04(InputSimulator inputSimulator)
    {
        inputSimulator.Keyboard.KeyUp(Up);
        inputSimulator.Keyboard.KeyUp(Down);
        inputSimulator.Keyboard.KeyUp(Left);
        inputSimulator.Keyboard.KeyUp(Right);
    }
}
