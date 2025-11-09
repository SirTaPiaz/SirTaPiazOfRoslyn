namespace Sample.Console;

internal sealed record Game
{
    public Guid Id { get; init; }
    public Guid PlayerId { get; init; }

    internal Game? Apply(GamesEvents @event)
    {
        return @event switch
        {
            GamesEvents.GameCreated created => new Game { Id = created.GameId },
            GamesEvents.PlayerJoined playerJoined => this with { PlayerId = playerJoined.PlayerId },
            GamesEvents.GameStarted => this,
            GamesEvents.GameEnded => this,
        };
    }

    internal void Apply(CustomerEvents @event)
    {
        var message = @event switch
        {
            CustomerEvents.CustomerActivated activated => "Activated",
            CustomerEvents.CustomerRegistered customerRegistered => "Registered",
        };

        System.Console.WriteLine(message);
    }
}

internal abstract record CustomerEvents
{
    private CustomerEvents()
    {
    }

    internal record CustomerRegistered : CustomerEvents;

    internal record CustomerActivated : CustomerEvents;
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