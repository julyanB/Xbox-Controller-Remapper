using ControllerRebinder.Common.Models.Configurations;
using Newtonsoft.Json;

namespace ControllerRebinder.Ui.Services;

public class ConfigurationService
{
    private readonly string _configPath = Path.Combine("..", "ControllerRebinder", "Configurations.json");

    public Configurations Load()
    {
        var json = File.ReadAllText(_configPath);
        return JsonConvert.DeserializeObject<Configurations>(json) ?? new Configurations();
    }

    public void Save(Configurations config)
    {
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(_configPath, json);
    }
}
