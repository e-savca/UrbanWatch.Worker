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
                                      ?? throw new ArgumentNullException("URBANWATCH_API_URL", "Missing API URL in configuration.");

    public async Task<int> SendVehicles(VehicleSnapshot vehicles)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_apiUrl, vehicles);

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
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while sending data to Urban Watch API");
            throw;
        }
    }
}