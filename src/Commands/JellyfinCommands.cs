using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using JellyfinDiscordBot.Model;

namespace JellyfinDiscordBot
{
    namespace Commands
    {
        public class JellyfinSearch : BaseCommandModule
        {
            public JellyfinCommunication? JellyCom { private get; set; }
            [Command("search")]
            public async Task SearchCommand(CommandContext ctx, [RemainingText] string? query)
            {
                if (query == null || query.Trim() == "")
                {
                    await ctx.RespondAsync("Usage: =search <query>");
                    return;
                }
                if (JellyCom != null)
                {

                    try
                    {
                        var songResults = await JellyCom.Search(query);
                        if (songResults == null || songResults.Count == 0)
                        {
                            await ctx.RespondAsync("Nothing found.");
                        }
                        else
                        {
                            var options = songResults.Select((song) => new DiscordSelectComponentOption(song.ToString(), song.Id.ToString()));
                            var dropdown = new DiscordSelectComponent("song_selection", null, options);
                            var builder = new DiscordMessageBuilder().WithContent("Select Song:")
                                                                     .WithReply(ctx.Message.Id)
                                                                     .AddComponents(dropdown);

                            var message = await builder.SendAsync(ctx.Channel);
                            var result = await message.WaitForSelectAsync("song_selection", new TimeSpan(0, 1, 0));
                            if (!result.TimedOut && result.Result.Values.Length > 0)
                            {
                                var songId = result.Result.Values.First();
                                await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage
                                , new DiscordInteractionResponseBuilder().WithContent($"Song selected: {songResults.Where(s => s.Id.ToString() == songId).First()}"));
                            }
                            else
                            {
                                await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage
                                , new DiscordInteractionResponseBuilder().WithContent("No song selected"));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException == null)
                        {
                            await ctx.RespondAsync($"Something went wrong {ex.Message}");

                        }
                        else
                        {
                            Exception? iter = ex;
                            StringBuilder sb = new();
                            while (iter != null)
                            {
                                sb.AppendLine(iter.Message);
                                iter = iter.InnerException;
                            }
                            _ = await ctx.RespondAsync($"Something went wrong {sb}");
                        }
                    }
                }
                else
                {
                    await ctx.RespondAsync("Hm, no Jellyfin Comm possible");

                }
            }
        }
    }
}
