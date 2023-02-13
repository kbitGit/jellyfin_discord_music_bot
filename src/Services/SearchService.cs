using System;
using System.Text;
using System.Threading.Tasks;

using Jellyfin.Sdk;

using SystemException = Jellyfin.Sdk.SystemException;

namespace Simple;


public class SearchService
{
    private readonly SdkClientSettings _sdkClientSettings;
    private readonly ISystemClient _systemClient;
    private readonly IUserClient _userClient;
    private readonly IUserViewsClient _userViewsClient;

    private bool ServerAvailable = false;
    private AuthenticationResult? authenticated;
    public string ServerURL { get; set; } = "";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";

    public SearchService(
        SdkClientSettings sdkClientSettings,
        ISystemClient systemClient,
        IUserClient userClient,
        IUserViewsClient userViewsClient)
    {
        _sdkClientSettings = sdkClientSettings;
        _systemClient = systemClient;
        _userClient = userClient;
        _userViewsClient = userViewsClient;
    }

    public async Task<string> RunAsync()
    {
        StringBuilder sb = new();


        _sdkClientSettings.BaseUrl = ServerURL;
        try
        {
            // Get public system info to verify that the url points to a Jellyfin server.
            var systemInfo = await _systemClient.GetPublicSystemInfoAsync()
                .ConfigureAwait(false);
            ServerAvailable = true;
            Console.WriteLine("Server Available");
        }
        catch (InvalidOperationException ex)
        {
            sb.AppendLine("Error connecting to server");
            await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            ServerAvailable = false;
        }
        catch (SystemException ex)
        {
            sb.AppendLine($"Error connecting to server");
            await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            ServerAvailable = false;
        }

        if (!ServerAvailable)
            return sb.ToString();

        if (authenticated == null)
        {
            try
            {

                Console.WriteLine($"Logging into {_sdkClientSettings.BaseUrl}");

                authenticated = await _userClient.AuthenticateUserByNameAsync(new AuthenticateUserByName
                {
                    Username = User,
                    Pw = Password
                })
                   .ConfigureAwait(false);
                _sdkClientSettings.AccessToken = authenticated.AccessToken;
                Console.WriteLine("Authentication success.");
            }
            catch (UserException ex)
            {
                sb.AppendLine("Error authenticating.");
                await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            }

        }
        else
        {
            _sdkClientSettings.AccessToken = authenticated.AccessToken;
            Console.WriteLine("Using Cached Login");
        }
        if (authenticated == null)
            return sb.ToString();

        await PrintViews(authenticated.User.Id, sb)
            .ConfigureAwait(false);
        return sb.ToString();
    }

    private async Task PrintViews(Guid userId, StringBuilder sb)
    {
        try
        {

            var views = await _userViewsClient.GetUserViewsAsync(userId)
                .ConfigureAwait(false);
            sb.AppendLine("Printing Views:");
            foreach (var view in views.Items)
            {
                sb.AppendLine($"{view.Id} - {view.Name}");
            }
        }
        catch (UserViewsException ex)
        {
            sb.AppendLine("Error getting user views");
            await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            authenticated = null;
        }
    }
}