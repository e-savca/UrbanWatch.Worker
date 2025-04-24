using UrbanWatch.Worker.ConfigManager;

namespace UrbanWatch.Worker.Interfaces;

public interface IEnvManager
{
    public TranzyApiKeyManager TranzyApiKeyManager { get; }
    public bool IsDevelopment();
}