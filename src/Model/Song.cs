namespace JellyfinDiscordBot.Model;

public record Song(Guid Id, string Name, string[] Artists)
{
    public override string ToString()
    {
        var aggregatedArtists = Artists.Aggregate("", (all, nextItem) => all.Length == 0 ? nextItem : all + "," + nextItem);
        return $"{Name} by {aggregatedArtists}";
    }
}