using System;
using ControllerRebinder.Common.Models.Configurations;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ControllerRebinder.Core.Caches
{
    public static class ConfigCache
    {
        private const string PATH = "Configurations.json";
        public static Configurations Configurations { get; set; }

        public static async Task InitAsync()
        {
            try
            {
                var configurations = await File.ReadAllTextAsync(PATH).ConfigureAwait(false);
                Configurations = JsonConvert.DeserializeObject<Configurations>(configurations);
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new Exception("Failed to initialize configuration.", ex);
            }
        }


        public static async Task Refresh()
        {
            var configurations = await File.ReadAllTextAsync(PATH).ConfigureAwait(false);
            var tempConf = JsonConvert.DeserializeObject<Configurations>(configurations);

            Configurations.LeftJoyStick = tempConf.LeftJoyStick;
            Configurations.RightJoyStick = tempConf.RightJoyStick;
            Configurations.Buttons = tempConf.Buttons;  
        }


    }
}
