namespace JellyfinDiscordBot
{
    class Program
    {
        static int Main()
        {
            var bot = new JellyfinDiscordBot();
            bot.Execute().GetAwaiter().GetResult();
            return 0;
        }
    }
}