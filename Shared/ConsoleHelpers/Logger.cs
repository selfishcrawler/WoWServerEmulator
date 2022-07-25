namespace Shared.ConsoleHelpers;

public static class Log
{
    private const ConsoleColor MessageColor = ConsoleColor.Green;
    private const ConsoleColor WarningColor = ConsoleColor.Yellow;
    private const ConsoleColor ErrorColor = ConsoleColor.Red;

    public static void Message(string message)
    {
        Console.ForegroundColor = MessageColor;
        Console.WriteLine(message);
    }


    public static void Warning(string message)
    {
        Console.ForegroundColor = WarningColor;
        Console.WriteLine(message);
    }


    public static void Error(string message)
    {
        Console.ForegroundColor = ErrorColor;
        Console.WriteLine(message);
    }

    public static void Message(object message) => Message(message.ToString());
    public static void Warning(object message) => Warning(message.ToString());
    public static void Error(object message) => Error(message.ToString());
}