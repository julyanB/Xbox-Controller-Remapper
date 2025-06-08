using ControllerRebinder.Ui.Services;
using ElectronNET.API;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseElectron(args);

builder.Services.AddControllersWithViews().AddNewtonsoftJson();
#if DEBUG
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
#endif
builder.Services.AddSingleton<ConfigurationService>();

var app = builder.Build();

if (HybridSupport.IsElectronActive)
{
    await Electron.WindowManager.CreateWindowAsync();
}

app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();

app.Run();
