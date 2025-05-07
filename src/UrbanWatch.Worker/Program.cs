using UrbanWatch.Worker;
using UrbanWatch.Worker.ConfigManager;
using UrbanWatch.Worker.Configuration;
using UrbanWatch.Worker.Helpers;
using UrbanWatch.Worker.HttpClients;
using UrbanWatch.Worker.Interfaces;
using UrbanWatch.Worker.Services;
using UrbanWatch.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddInfisical(
    token: builder.Configuration["INFISICAL_TOKEN"] ?? throw new InvalidOperationException("INFISICAL_TOKEN missing"),
    workspaceId: builder.Configuration["INFISICAL_WORKSPACE_ID"] ??
                 throw new InvalidOperationException("INFISICAL_WORKSPACE_ID missing"),
    tag: builder.Configuration["INFISICAL_TAG"] ??
         throw new InvalidOperationException("INFISICAL_TAG missing"),
    environment: builder.Configuration["INFISICAL_ENVIRONMENT"] ?? "prod",
    baseUrl: builder.Configuration["INFISICAL_URL"] ?? "http://vault.home"
);


builder.Services.AddHttpClient();
builder.Services.AddHostedService<FetchVehiclesWorker>();
// builder.Services.AddHostedService<CleanupVehiclesLive>();
// builder.Services.AddHostedService<PullTranzyData>();


// Register dependencies
builder.Services.AddTransient<TranzyApiKeyManager>();

builder.Services.AddSingleton<TranzyClient>();
builder.Services.AddSingleton<UrbanWatchClient>();
builder.Services.AddSingleton<IEnvManager, EnvManager>();
builder.Services.AddSingleton<TimeWindowHelper>();

builder.Services.AddSingleton<FetchVehicleService>();
// builder.Services.AddSingleton<VehicleHistoryService>();

var host = builder.Build();
host.Run();