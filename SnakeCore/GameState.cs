namespace SnakeCore;

public class GameState
{
    private readonly GameStateSource gameStateSource;

    public float ElapsedSeconds => gameStateSource.ElapsedSeconds;

    public Direction Direction => gameStateSource.Direction;

    public GameState(GameStateSource gameStateSource)
    {
        this.gameStateSource = gameStateSource;
    }
}

public class GameStateSource
{
    public float ElapsedSeconds { get; set; }

    public Direction Direction { get; set; }
}
