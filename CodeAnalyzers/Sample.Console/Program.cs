string input;

do
{
    Console.WriteLine("Enter name: ");
    input = Console.ReadLine()!;

    var greet = Greeter.Greet(input);

    _ = greet.Match(() =>
    {
        Console.WriteLine("Hello, World!");
        return 0;
    }, s =>
    {
        Console.WriteLine(s);
        return 1;
    });
} while (input != "exit");
