using UrbanWatch.Worker;
using UrbanWatch.Worker.ConfigManager;
using UrbanWatch.Worker.Configuration;
using UrbanWatch.Worker.Infrastructure.HttpClients;
using UrbanWatch.Worker.Infrastructure.Mongo;
using UrbanWatch.Worker.Infrastructure.Redis;
using UrbanWatch.Worker.Interfaces;
using UrbanWatch.Worker.Services;
using UrbanWatch.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddInfisical(
    token:       builder.Configuration["INFISICAL_TOKEN"] ?? throw new InvalidOperationException("INFISICAL_TOKEN missing"),
    workspaceId: builder.Configuration["INFISICAL_WORKSPACE_ID"] ?? throw new InvalidOperationException("INFISICAL_WORKSPACE_ID missing"),
    environment: builder.Configuration["INFISICAL_ENVIRONMENT"] ?? "prod",
    folder:      "/",
    baseUrl:     builder.Configuration["INFISICAL_URL"] ?? "http://vault.home"
);

builder.Services.AddHttpClient();
builder.Services.AddHostedService<FetchVehiclesWorker>();
builder.Services.AddHostedService<CleanupVehiclesLive>();
builder.Services.AddHostedService<PullTranzyData>();

// bind env vars for MongoSetting
builder.Services.Configure<MongoSettings>(options =>
{
    options.Host     = builder.Configuration["MONGO_HOST"]     ?? throw new Exception("MONGO_HOST missing");
    options.Port     = int.Parse(builder.Configuration["MONGO_PORT"] ?? "0");
    options.Database = builder.Configuration["MONGO_DATABASE"] ?? throw new Exception("MONGO_DATABASE missing");
    options.Username = builder.Configuration["MONGO_USERNAME"] ?? throw new Exception("MONGO_USERNAME missing");
    options.Password = builder.Configuration["MONGO_PASSWORD"] ?? throw new Exception("MONGO_PASSWORD missing");
});

// Register dependencies
builder.Services.AddTransient<TranzyApiKeyManager>();

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<RedisContext>();

builder.Services.AddSingleton<TranzyClient>();
builder.Services.AddSingleton<IEnvManager, EnvManager>();
builder.Services.AddSingleton<TimeWindowHelper>();

builder.Services.AddSingleton<VehicleHistoryService>();
builder.Services.AddSingleton<MongoCollectionService>();
builder.Services.AddSingleton<CacheService>();

var host = builder.Build();
host.Run();
