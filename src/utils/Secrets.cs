namespace JellyfinDiscordBot.Utils;
public class Secrets
{
    public string DiscordToken { get; set; } = "";
    public string JellyfinUrl { get; set; } = "";
    public string JellyfinUser { get; set; } = "";
    public string JellyfinPassword { get; set; } = "";
    public bool MembersEmpty()
    {
        return DiscordToken.Trim() == "" || JellyfinUser.Trim() == "" || JellyfinPassword.Trim() == "";
    }
}