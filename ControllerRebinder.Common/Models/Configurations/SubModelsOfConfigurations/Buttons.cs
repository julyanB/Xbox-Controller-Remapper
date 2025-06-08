using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WindowsInput.Native;

namespace ControllerRebinder.Common.Models.Configurations.SubModelsOfConfigurations
{
    public class Buttons
    {
        public bool On { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualKeyCode X { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualKeyCode Y { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualKeyCode A { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualKeyCode B { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualKeyCode DPadUp { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualKeyCode DPadDown { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualKeyCode DPadLeft { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public VirtualKeyCode DPadRight
        {
            get; set;
        }
    }
}
