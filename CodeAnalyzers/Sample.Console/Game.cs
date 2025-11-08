namespace Sample.Console;

internal sealed record Game
{
    public Guid Id { get; init; }    
    public Guid PlayerId { get; init; }    
    internal Game? Apply(GamesEvents @event)
    {
        return @event switch
        {
            GamesEvents.GameCreated created => new Game{ Id = created.GameId },
            GamesEvents.PlayerJoined playerJoined => this with{PlayerId = playerJoined.PlayerId},
            GamesEvents.GameStarted => this,
            GamesEvents.GameEnded => this,
        };
    }
}

internal abstract record GamesEvents
{
    private GamesEvents()
    {
    }

    internal record GameCreated(Guid GameId) : GamesEvents;

    internal record PlayerJoined(Guid GameId, Guid PlayerId) : GamesEvents;

    internal record GameStarted(Guid GameId) : GamesEvents;

    internal record GameEnded(Guid GameId) : GamesEvents;
}