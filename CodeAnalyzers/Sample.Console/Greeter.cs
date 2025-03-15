using Sample.Fx;

public class Greeter
{
    public static Maybe<string> Greet(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(@"Name cannot be null or whitespace", nameof(name));
        }

        return $"Hello, {name}";
    }
}
