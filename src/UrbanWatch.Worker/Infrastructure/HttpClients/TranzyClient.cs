using System.Net;
using Newtonsoft.Json;
using UrbanWatch.Worker.Infrastructure.Mongo.Documents;
using UrbanWatch.Worker.Interfaces;

namespace UrbanWatch.Worker.Infrastructure.HttpClients;

public class TranzyClient(
    IEnvManager envManager,
    IHttpClientFactory factory,
    ILogger<TranzyClient> logger
    )
{
    private IEnvManager EnvManager { get; } = envManager;

    private async Task<List<T>> GetGenericAsync<T>(HttpRequestMessage request)
    {
        using var client = factory.CreateClient();

        try
        {
            var response = await SendRequestAsync(client, request, logger);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                logger.LogWarning("Status Code 429: Too Many Requests");

                bool switched = EnvManager.TranzyApiKeyManager.TrySwitchKey();
                if (!switched)
                    throw new Exception("All API keys exceeded quota.");
                
                var retryRequest = CloneRequestWithNewApiKey(request, EnvManager.TranzyApiKeyManager.GetCurrentKey());

                var retryResponse = await SendRequestAsync(client, retryRequest, logger);
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
        catch (Exception e)
        {
            logger.LogError(e, "Error while calling API.");
            throw;
        }
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpClient client, HttpRequestMessage request, ILogger logger)
    {
        var response = await client.SendAsync(request);
        logger.LogInformation($"Received status code: {(int)response.StatusCode} ({response.StatusCode})");
        return response;
    }

    private HttpRequestMessage CloneRequestWithNewApiKey(HttpRequestMessage originalRequest, string newApiKey)
    {
        var clone = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri)
        {
            Content = originalRequest.Content,
            Version = originalRequest.Version
        };

        foreach (var header in originalRequest.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        clone.Headers.Remove("X-API-KEY");
        clone.Headers.Add("X-API-KEY", newApiKey);

        return clone;
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