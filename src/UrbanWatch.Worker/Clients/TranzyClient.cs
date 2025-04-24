using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using UrbanWatch.Worker.ConfigManager;
using UrbanWatch.Worker.Documents;
using UrbanWatch.Worker.Interfaces;

namespace UrbanWatch.Worker.Clients;

public class TranzyClient
{
    public IEnvManager EnvManager { get; }
    private readonly IHttpClientFactory _factory;

    private readonly ILogger<TranzyClient> _logger;

    public TranzyClient(
        IEnvManager envManager,
        IHttpClientFactory factory,
        ILogger<TranzyClient> logger)
    {
        EnvManager = envManager;
        _factory = factory;
        _logger = logger;
    }

    public async Task<List<T>> GetGenericAsync<T>(HttpRequestMessage request)
    {
        using var client = _factory.CreateClient();

        try
        {
            using (var response = await client.SendAsync(request))
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Status Code 429: Too Many Requests");
                    bool switched = EnvManager.TranzyApiKeyManager.TrySwitchKey();

                    if (!switched)
                        throw new Exception("All API keys exceeded quota.");

                    request.Headers.Remove("X-API-KEY");
                    request.Headers.Add("X-API-KEY", EnvManager.TranzyApiKeyManager.GetCurrentKey());

                    using var retryResponse = await client.SendAsync(request);
                    retryResponse.EnsureSuccessStatusCode();
                    var retryBody = await retryResponse.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<T>>(retryBody) ?? new List<T>();
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<T>>(body) ?? new List<T>();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while calling API.");
            throw;
        }
    }


    public async Task<List<Vehicle>> GetVehiclesAsync(string agencyId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.tranzy.ai/v1/opendata/vehicles"),
            Headers =
                {
                    { "X-Agency-Id", agencyId },
                    { "Accept", "application/json" },
                    { "X-API-KEY", EnvManager.TranzyApiKeyManager.GetCurrentKey() },
                },
        };

        return await GetGenericAsync<Vehicle>(request);
    }

    public async Task<List<Route>> GetRoutesAsync(string agencyId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.tranzy.ai/v1/opendata/routes"),
            Headers = {
                    { "X-Agency-Id", agencyId },
                    { "Accept", "application/json" },
                    { "X-API-KEY", EnvManager.TranzyApiKeyManager.GetCurrentKey() },
                }
        };
        return await GetGenericAsync<Route>(request);

    }

    public async Task<List<Shape>> GetShapeAsync(string agencyId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.tranzy.ai/v1/opendata/shapes"),
            Headers = {
                    { "X-Agency-Id", agencyId },
                    { "Accept", "application/json" },
                    { "X-API-KEY", EnvManager.TranzyApiKeyManager.GetCurrentKey() },
                }
        };

        return await GetGenericAsync<Shape>(request);

    }

    public async Task<List<Stop>> GetStopsAsync(string agencyId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.tranzy.ai/v1/opendata/stops"),
            Headers = {
                    { "X-Agency-Id", agencyId },
                    { "Accept", "application/json" },
                    { "X-API-KEY", EnvManager.TranzyApiKeyManager.GetCurrentKey() },
                }
        };

        return await GetGenericAsync<Stop>(request);
    }

    public async Task<List<StopTimes>> GetStopTimesAsync(string agencyId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.tranzy.ai/v1/opendata/stop_times"),
            Headers = {
                    { "X-Agency-Id", agencyId },
                    { "Accept", "application/json" },
                    { "X-API-KEY", EnvManager.TranzyApiKeyManager.GetCurrentKey() },
                }
        };

        return await GetGenericAsync<StopTimes>(request);
    }

    public async Task<List<Trip>> GetTripsAsync(string agencyId)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.tranzy.ai/v1/opendata/trips"),
            Headers = {
                    { "X-Agency-Id", agencyId },
                    { "Accept", "application/json" },
                    { "X-API-KEY", EnvManager.TranzyApiKeyManager.GetCurrentKey() },
                }
        };

        return await GetGenericAsync<Trip>(request);
    }
}