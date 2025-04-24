using System;
using UrbanWatch.Worker.Interfaces;

namespace UrbanWatch.Worker.ConfigManager;

public class EnvManager : IEnvManager
{
    public TranzyApiKeyManager TranzyApiKeyManager { get; }

    private IConfiguration Config { get; }

    public EnvManager(
        IConfiguration config
    )
    {
        TranzyApiKeyManager = new TranzyApiKeyManager(config);
        Config = config;
    }
    
    public bool IsDevelopment() => Config["ASPNETCORE_ENVIRONMENT"] == "Development";

}
