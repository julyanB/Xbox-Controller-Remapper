using ControllerRebinder.Ui.Services;
using ControllerRebinder.Ui.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ControllerRebinder.Ui.Controllers;

public class ConfigurationsController : Controller
{
    private readonly ConfigurationService _service;

    public ConfigurationsController(ConfigurationService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var vm = new ConfigurationsViewModel { Config = _service.Load() };
        return View(vm);
    }

    [HttpPost]
    public IActionResult Index(ConfigurationsViewModel model)
    {
        if (ModelState.IsValid)
        {
            _service.Save(model.Config);
            ViewBag.Message = "Saved";
        }
        return View(model);
    }
}
