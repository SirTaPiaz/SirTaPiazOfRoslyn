namespace CreatingSyntaxTrees;

internal static class ConsoleExtensions
{
    internal static void WriteLine(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
