namespace SnakeWebGL;

public partial class Keys
{
    public static Keys W = new Keys("KeyW");

    public static Keys D = new Keys("KeyD");

    public static Keys S = new Keys("KeyS");

    public static Keys A = new Keys("KeyA");

    public readonly string Value;

    private Keys(string value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    public static implicit operator string(Keys key) => key.Value;
}
