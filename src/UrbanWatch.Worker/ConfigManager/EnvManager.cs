using System;
using UrbanWatch.Worker.Interfaces;

namespace UrbanWatch.Worker.ConfigManager;

public class EnvManager(
    TranzyApiKeyManager tranzyApiKeyManager,
    IConfiguration config
    ) : IEnvManager
{
    public TranzyApiKeyManager TranzyApiKeyManager { get; } = tranzyApiKeyManager;

    public bool IsDevelopment() => config["DOTNET_ENVIRONMENT"] == "Development";

}
