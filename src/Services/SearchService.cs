using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Jellyfin.Sdk;

using JellyfinDiscordBot.Model;

using SystemException = Jellyfin.Sdk.SystemException;

namespace Simple;


public class SearchService
{
    private readonly SdkClientSettings _sdkClientSettings;
    private readonly ISystemClient _systemClient;
    private readonly IUserClient _userClient;
    private readonly ISearchClient _searchClient;

    private AuthenticationResult? _authenticated;
    public string ServerURL { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";

    public SearchService(
        SdkClientSettings sdkClientSettings,
        ISystemClient systemClient,
        IUserClient userClient,
        ISearchClient searchClient)
    {
        _sdkClientSettings = sdkClientSettings;
        _systemClient = systemClient;
        _userClient = userClient;
        _searchClient = searchClient;
    }

    public async Task<IReadOnlyList<Song>> RunAsync(string query)
    {
        _sdkClientSettings.BaseUrl = ServerURL;
        bool serverAvailable;
        try
        {
            // Get public system info to verify that the url points to a Jellyfin server.
            var systemInfo = await _systemClient.GetPublicSystemInfoAsync()
                .ConfigureAwait(false);
            serverAvailable = true;
            Console.WriteLine("Server Available");
        }
        catch (InvalidOperationException ex)
        {
            await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            serverAvailable = false;
        }
        catch (SystemException ex)
        {
            await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            serverAvailable = false;
        }

        if (!serverAvailable)
            return new List<Song>();

        if (_authenticated == null)
        {
            try
            {

                Console.WriteLine($"Logging into {_sdkClientSettings.BaseUrl}");

                _authenticated = await _userClient.AuthenticateUserByNameAsync(new AuthenticateUserByName
                {
                    Username = User,
                    Pw = Password
                })
                   .ConfigureAwait(false);
                _sdkClientSettings.AccessToken = _authenticated.AccessToken;
                Console.WriteLine("Authentication success.");
            }
            catch (UserException ex)
            {
                await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            }

        }
        else
        {
            _sdkClientSettings.AccessToken = _authenticated.AccessToken;
            Console.WriteLine("Using Cached Login");
        }
        return _authenticated == null
            ? new List<Song>()
            : await SearchSong(_authenticated.User.Id, query)
            .ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<Song>> SearchSong(Guid userId, string query)
    {
        try
        {
            var res = await _searchClient.GetAsync(query, null, null, userId, new BaseItemKind[] { BaseItemKind.Audio });
            return res.SearchHints.Select((hint) => new Song(hint.Id,
                                                             hint.Name,
                                                             hint.Artists.ToArray())).ToList();
        }
        catch (UserViewsException ex)
        {
            await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            _authenticated = null;
            return new List<Song>();
        }
    }
}