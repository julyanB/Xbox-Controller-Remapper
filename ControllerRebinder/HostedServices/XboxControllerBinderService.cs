using System.Threading;
using System.Threading.Tasks;
using ControllerRebinder.Core;
using Microsoft.Extensions.Hosting;

namespace ControllerRebinder.TesterConsole.HostedServices;

public class XboxControllerBinderService : IHostedService
{
    private readonly XboxControllerBinder _binder;

    public XboxControllerBinderService(XboxControllerBinder binder)
    {
        _binder = binder;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _binder.Start(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}