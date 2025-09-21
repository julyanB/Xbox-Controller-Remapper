using ControllerRebinder.Core.Abstractions;
using ControllerRebinder.Core.Adapters;
using ControllerRebinder.Core.Configuration;
using ControllerRebinder.Core.Services;
using DXNET.XInput;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WindowsInput;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile(
    "Configurations.json",
    optional: false,
    reloadOnChange: true);

builder.Services.AddOptions<ControllerRemapperOptions>()
    .Bind(builder.Configuration.GetSection(ControllerRemapperOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<ControllerRemapperOptions>, ControllerRemapperOptionsValidator>();

builder.Services.AddSingleton<InputSimulator>();
builder.Services.AddSingleton<IKeyboardEmulator, KeyboardSimulatorAdapter>();

builder.Services.AddSingleton<IXboxController>(sp =>
{
    var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<ControllerRemapperOptions>>();
    var controllerIndex = Math.Clamp(optionsMonitor.CurrentValue.ControllerIndex, 0, 3);
    var userIndex = controllerIndex switch
    {
        0 => UserIndex.One,
        1 => UserIndex.Two,
        2 => UserIndex.Three,
        _ => UserIndex.Four
    };

    return new XboxControllerAdapter(userIndex);
});

builder.Services.AddHostedService<ControllerRemappingService>();

builder.Logging.AddConsole();

using var host = builder.Build();

var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
var bootstrapLogger = host.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("ControllerRebinder");

lifetime.ApplicationStarted.Register(() =>
{
    bootstrapLogger.LogInformation("Controller remapper running. Press Ctrl+C to exit.");
});

await host.RunAsync();
