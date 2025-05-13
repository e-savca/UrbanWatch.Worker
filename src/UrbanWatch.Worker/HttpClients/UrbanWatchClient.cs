using System.Net;
using System.Net.Http.Json;
using UrbanWatch.Worker.DTOs;

namespace UrbanWatch.Worker.HttpClients;

public class UrbanWatchClient(
    IConfiguration config,
    IHttpClientFactory factory,
    ILogger<UrbanWatchClient> logger
)
{
    private readonly HttpClient _httpClient = factory.CreateClient("UrbanWatch");

    private readonly string _apiUrl = config["URBANWATCH_API_URL"]
                                      ?? throw new ArgumentNullException("URBANWATCH_API_URL",
                                          "Missing API URL in configuration.");

    private async Task<HttpResponseMessage> GetAsync(string url)
    {
        try
        {
            return await _httpClient.GetAsync(url);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[GET] Request to {Url} failed", url);
            throw;
        }
    }

    private async Task<HttpResponseMessage> PostAsync<TPayload>(string url, TPayload payload)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, "[POST] Request to {Url} failed", url);
            throw;
        }
    }

    private async Task<HttpResponseMessage> PutAsync<TPayload>(string url, TPayload payload)
    {
        try
        {
            return await _httpClient.PutAsJsonAsync(url, payload);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[PUT] Request to {Url} failed", url);
            throw;
        }
    }

    private async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        try
        {
            return await _httpClient.DeleteAsync(url);
        }
        catch (Exception e)
        {
            logger.LogError(e, "[DELETE] Request to {Url} failed", url);
            throw;
        }
    }


    public async Task<int> SendVehiclesAsync(VehicleSnapshot vehicles)
    {
        var url = $"{_apiUrl}/map/Vehicles";

        try
        {
            var response = await PostAsync(url, vehicles);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    logger.LogInformation("Successfully sent vehicle data to Urban Watch API.");
                    return 1;
                }

                logger.LogWarning("Unexpected status code: {StatusCode}", response.StatusCode);
                return 0;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to send data. Status: {StatusCode}, Body: {Body}",
                response.StatusCode, errorContent);
            return 0;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception while sending data to Urban Watch API");
            throw;
        }
    }

    public async Task ClearVehiclesAsync(TimeSpan timeSpan)
    {
        var url = $"{_apiUrl}/map/Vehicles?ticks={timeSpan.Ticks}";

        try
        {
            await DeleteAsync(url);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw;
        }
    }
}