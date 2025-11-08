using Sample.Fx;

public class Greeter
{
    public static Maybe<string> Greet(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Maybe.None;
        }

        return $"Hello, {name}";
    }
}
