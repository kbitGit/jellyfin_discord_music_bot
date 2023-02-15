
using System.Text.Json;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity.Extensions;

using JellyfinDiscordBot.Commands;
using JellyfinDiscordBot.Utils;

using Microsoft.Extensions.DependencyInjection;

namespace JellyfinDiscordBot;
class JellyfinDiscordBot
{
    private readonly DiscordClient _discord;  //"B5NaT8s9HqRN2js"

    public JellyfinDiscordBot()
    {
        Secrets? secrets = null;
        if (File.Exists("secrets.json"))
        {
            var jsonContent = File.ReadAllText("secrets.json");
            if (jsonContent == "")
            {
                throw new InvalidDataException("secrets file cannot be empty.");
            }
            secrets = JsonSerializer.Deserialize<Secrets>(jsonContent);
            if (secrets == null || secrets.MembersEmpty())
            {
                throw new Exception("Secret or one of its members was deserialized to null");
            }
        }
        var jellyfinCom = new JellyfinCommunication(secrets!);
        var commandServices = new ServiceCollection()
        .AddSingleton(jellyfinCom)
        .BuildServiceProvider();

        _discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = secrets!.DiscordToken,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        });
        _discord.UseInteractivity();
        var commands = _discord.UseCommandsNext(new CommandsNextConfiguration()
        {
            StringPrefixes = new[] { "=" },
            Services = commandServices
        });
        commands.RegisterCommands<JellyfinSearch>();
    }
    public async Task Execute()
    {
        await _discord.ConnectAsync();
        await Task.Delay(-1);
    }
}
