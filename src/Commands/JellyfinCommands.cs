using System.Text;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace JellyfinDiscordBot
{
    namespace Commands
    {
        public class JellyfinSearch : BaseCommandModule
        {
            public JellyfinCommunication? JellyCom { private get; set; }
            [Command("search")]
            public async Task SearchCommand(CommandContext ctx)
            {
                if (JellyCom != null)
                {

                    try
                    {
                        var jellyfinViews = await JellyCom.Execute();
                        if (jellyfinViews.Trim() == "")
                        {
                            await ctx.RespondAsync("Nothing found.");

                        }
                        else
                        {
                            await ctx.RespondAsync(jellyfinViews);
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
