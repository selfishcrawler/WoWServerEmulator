using System.Runtime.InteropServices;

namespace Shared.ConsoleHelpers;

public abstract class ConsoleCommandHandler
{
    private enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(StdHandle std);
    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hdl);

    protected delegate void CommandHandler(ReadOnlySpan<string> args);
    protected Dictionary<string, CommandHandler> Commands { get; init; }

    public ConsoleCommandHandler(CancellationToken token)
    {
        token.Register(StopReading);
    }

    private void StopReading()
    {
        var handle = GetStdHandle(StdHandle.Stdin);
        CloseHandle(handle);
    }

    protected void HandleSubCommand(Dictionary<string, CommandHandler> subHandlers, ReadOnlySpan<string> args, string cmd)
    {
        if (args == ReadOnlySpan<string>.Empty || !subHandlers.ContainsKey(args[0]))
        {
            Log.Error("Command doesn't exist");
            Log.Warning($"Possible {cmd} commands: ");
            foreach (var c in subHandlers)
            {
                Log.Warning($"\t{c.Key}");
            }
            return;
        }

        subHandlers[args[0]](args.Length > 1 ? args[1..] : ReadOnlySpan<string>.Empty);
    }

    public void Handle()
    {
        string line;
        while (true)
        {
            try
            {
                line = Console.ReadLine();
            }
            catch (IOException)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
                continue;
            ReadOnlySpan<string> split = line.Split(' '); // use custom span split ?

            HandleSubCommand(Commands, split, split[0]);
        }
    }
}
