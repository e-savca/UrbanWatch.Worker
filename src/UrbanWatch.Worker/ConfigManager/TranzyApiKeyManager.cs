namespace UrbanWatch.Worker.ConfigManager;

public class TranzyApiKeyManager
{
    private const string TRANZY_API_KEY_1 = "TRANZY_API_KEY_1";
    private const string TRANZY_API_KEY_2 = "TRANZY_API_KEY_2";
    private const string TRANZY_API_KEY_3 = "TRANZY_API_KEY_3";
    private readonly IConfiguration _config;
    private readonly List<string> _apiKeys = new List<string>();
    private int _currentKeyIndex = 0;

    public TranzyApiKeyManager(IConfiguration config)
    {
        _config = config;
        Initialize();
    }

    private void Initialize()
    {
        if (IsDevelopment())
        {
            _apiKeys.Add(_config[TRANZY_API_KEY_1]);
        }
        else
        {
            _apiKeys.Add(_config[TRANZY_API_KEY_1]);
            _apiKeys.Add(_config[TRANZY_API_KEY_2]);
            _apiKeys.Add(_config[TRANZY_API_KEY_3]);
        }
        
    }

    private bool IsDevelopment() => _config["DOTNET_ENVIRONMENT"] == "Development";
    
    public string GetCurrentKey() => _apiKeys[_currentKeyIndex];

    public bool TrySwitchKey()
    {
        if (IsDevelopment()) return false;
        if (_currentKeyIndex + 1 < _apiKeys.Count)
        {
            _currentKeyIndex++;
            return true;
        }
        else
        {
            _currentKeyIndex = 0;
            return true;
        }
    }
}
