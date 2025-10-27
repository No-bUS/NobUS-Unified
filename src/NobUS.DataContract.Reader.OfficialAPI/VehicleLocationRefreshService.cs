using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace NobUS.DataContract.Reader.OfficialAPI;

internal sealed class VehicleLocationRefreshService : BackgroundService
{
    private readonly CongestedClient _client;

    public VehicleLocationRefreshService(CongestedClient client)
    {
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _client.EnsureInitializedAsync(stoppingToken).ConfigureAwait(false);

        using var timer = new PeriodicTimer(CongestedClient.UpdateInterval);
        do
        {
            await _client.RefreshVehicleLocationsAsync(stoppingToken).ConfigureAwait(false);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }
}
