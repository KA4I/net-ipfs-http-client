namespace Ipfs.Http.Client;

using Ipfs.CoreApi;
using Ipfs.Http.Client.CoreApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using System.Net;
using System.Reflection;

/// <summary>
/// The service collection extensions class.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a custom configured HTTP client for IPFS.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="ipfsApiUrl">The IPFS API URL.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// The default URL to the IPFS HTTP API server is <c>http://localhost:5001</c>. The environment variable "IpfsApiUrl" can be used to override this default.
    /// </remarks>
    public static IServiceCollection AddIpfsClient(this IServiceCollection services, Uri? ipfsApiUrl = null)
    {
        ipfsApiUrl ??= new Uri(Environment.GetEnvironmentVariable("IpfsApiUrl") ?? "http://localhost:5001");

        // The user agent value is "net-ipfs/M.N", where M is the major and N is minor version numbers of the assembly.
        var version = typeof(IpfsClient).GetTypeInfo().Assembly.GetName().Version;
        var userAgent = $"net-ipfs/{version?.Major ?? 0}.{version?.Minor ?? 0}";

        _ = services
            .AddHttpClient(
                IpfsClient.IpfsHttpClientName,
                client =>
                {
                    client.BaseAddress = ipfsApiUrl;
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                    client.Timeout = Timeout.InfiniteTimeSpan;
                })
            .ConfigurePrimaryHttpMessageHandler(
                () =>
                {
                    var handler = new HttpClientHandler();
                    if (handler.SupportsAutomaticDecompression)
                    {
                        handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    }

                    return handler;
                })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(5);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(11);
            });

        _ = services
            .AddLogging();

        _ = services
            .AddScoped<IBitswapApi, BitswapApi>()
            .AddScoped<IBlockApi, BlockApi>()
            .AddScoped<IBlockRepositoryApi, BlockRepositoryApi>()
            .AddScoped<IBootstrapApi, BootstrapApi>()
            .AddScoped<IConfigApi, ConfigApi>()
            .AddScoped<IDagApi, DagApi>()
            .AddScoped<IDhtApi, DhtApi>()
            .AddScoped<IDnsApi, DnsApi>()
            .AddScoped<IFileSystemApi, FileSystemApi>()
            .AddScoped<IFilestoreApi, FilestoreApi>()
            .AddScoped<IGenericApi, GenericApi>()
            .AddScoped<IIpfsClient, IpfsClient>()
            .AddScoped<IKeyApi, KeyApi>()
            .AddScoped<IMfsApi, MfsApi>()
            .AddScoped<INameApi, NameApi>()
            .AddScoped<IPinApi, PinApi>()
            .AddScoped<IPubSubApi, PubSubApi>()
            .AddScoped<IStatsApi, StatsApi>()
            .AddScoped<ISwarmApi, SwarmApi>()
            .AddScoped<IpfsContext>();

        return services;
    }
}
