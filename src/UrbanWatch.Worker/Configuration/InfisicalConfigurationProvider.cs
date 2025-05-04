using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace UrbanWatch.Worker.Configuration;

public class InfisicalConfigurationProvider : ConfigurationProvider
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)  
    };

    private readonly string _token;
    private readonly string _workspaceId;
    private readonly string _environment;
    private readonly string _folder;
    private readonly string _baseUrl;

    public InfisicalConfigurationProvider(
        string token,
        string workspaceId,
        string environment,
        string folder,
        string baseUrl)
    {
        _token = token ?? throw new ArgumentNullException(nameof(token));
        _workspaceId = workspaceId ?? throw new ArgumentNullException(nameof(workspaceId));
        _environment = environment;
        _folder = folder;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public override void Load()
    {
        var url = $"{_baseUrl}/api/v3/secrets/raw" +
                  $"?environment={Uri.EscapeDataString(_environment)}" +
                  $"&workspaceId={Uri.EscapeDataString(_workspaceId)}" +
                  $"&path={Uri.EscapeDataString(_folder)}";

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var resp = _httpClient.Send(req);
                resp.EnsureSuccessStatusCode();

                var body = resp.Content.ReadAsStringAsync();
                
                var responseBody = resp.Content.ReadAsStringAsync()
                    .GetAwaiter()
                    .GetResult();
                
                var payload = JsonConvert.DeserializeObject<InfisicalResponse>(responseBody) ?? throw new InvalidOperationException("Empty payload");
                Data = payload.secrets.ToDictionary(s => s.secretKey, s => s.secretValue); 
                return;
            }
            catch (Exception ex) when (attempt < 3)
            {
                Thread.Sleep(500 * attempt);
            }
        }

        throw new InvalidOperationException("Could not load secrets from Infisical after 3 attempts.");
    }

    private class InfisicalResponse
    {
        public List<Secret> secrets { get; set; } = new();
    }

    private class Secret
    {
        public string secretKey { get; set; }
        public string secretValue { get; set; }
    }
}