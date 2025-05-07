namespace UrbanWatch.Worker.Configuration;

public class InfisicalConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new InfisicalConfigurationProvider(
            token: Token, 
            workspaceId: WorkspaceId,
            environment: Environment,
            tag: Tag,
            baseUrl: BaseUrl);

    public string Token       { get; init; }
    public string WorkspaceId { get; init; }
    public string Environment { get; init; }
    public string Tag      { get; init; }
    public string BaseUrl     { get; init; }
}

public static class InfisicalConfigurationExtensions
{
    public static IConfigurationBuilder AddInfisical(
        this IConfigurationBuilder builder,
        string token,
        string workspaceId,
        string tag,
        string environment        = "prod",
        string baseUrl            = "http://vault.home")
    {
        var src = new InfisicalConfigurationSource
        {
            Token       = token,
            WorkspaceId = workspaceId,
            Tag      = tag,
            Environment = environment,
            BaseUrl     = baseUrl
        };
        return builder.Add(src);
    }
}