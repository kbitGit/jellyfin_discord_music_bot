using Jellyfin.Sdk;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Mime;
using System.Net.Security;
using JellyfinDiscordBot.Model;

namespace JellyfinDiscordBot
{
    public class JellyfinCommunication
    {
        private readonly Simple.SearchService _service;
        public JellyfinCommunication(Utils.Secrets secrets)
        {
            var serviceProvider = ConfigureServices();
            var sdkClientSettings = serviceProvider.GetRequiredService<SdkClientSettings>();
            sdkClientSettings.InitializeClientSettings(
                "Discord Client",
                "0.0.1",
                "Discord",
                $"this-is-my-device-id-{Guid.NewGuid():N}");

            _service = serviceProvider.GetRequiredService<Simple.SearchService>();
            _service.ServerURL = secrets.JellyfinUrl;
            _service.User = secrets.JellyfinUser;
            _service.Password = secrets.JellyfinPassword;
        }
        public Task<IReadOnlyList<Song>> Search(string query)
        {
            var task = _service.RunAsync(query);
            _ = task.ConfigureAwait(false);
            return task;
        }
        private static ServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            static SocketsHttpHandler DefaultHttpClientHandlerDelegate(IServiceProvider service)
            {
                return new()
                {
                    AutomaticDecompression = DecompressionMethods.All,
                    RequestHeaderEncodingSelector = (_, _) => System.Text.Encoding.UTF8,
                    SslOptions = new SslClientAuthenticationOptions()
                    {
                        RemoteCertificateValidationCallback = delegate { return true; }

                    },
                };
            }

            serviceCollection.AddHttpClient("Default",
                                                    c =>
                                                    {
                                                        c.DefaultRequestHeaders.UserAgent.Add(
                                                            new ProductInfoHeaderValue("Jellyfin-Discord-Bot", "0.0.1"));
                                                        c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json, 1.0));
                                                        c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
                                                    })
                                                    .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            serviceCollection
            .AddSingleton<SdkClientSettings>();
            serviceCollection
            .AddHttpClient<ISystemClient, SystemClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            serviceCollection
            .AddHttpClient<IUserClient, UserClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            serviceCollection
            .AddHttpClient<IUserViewsClient, UserViewsClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            serviceCollection
            .AddHttpClient<IUserLibraryClient, UserLibraryClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            serviceCollection
            .AddHttpClient<ISearchClient, SearchClient>()
            .ConfigurePrimaryHttpMessageHandler(DefaultHttpClientHandlerDelegate);
            serviceCollection.AddSingleton<Simple.SearchService>();
            return serviceCollection.BuildServiceProvider();
        }
    }
}