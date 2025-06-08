using ControllerRebinder.Common.Models.Configurations.SubModelsOfConfigurations;

namespace ControllerRebinder.Common.Models.Configurations
{
    public class Configurations
    {
        public int RefreshRate { get; set; }
        public bool Log { get; set; }
        public BaseJoyStick LeftJoyStick { get; set; }
        public BaseJoyStick RightJoyStick { get; set; }
        public Buttons Buttons { get; set; }
    }
}
