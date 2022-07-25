using AuthServer.Cryptography;
using AuthServer.Network;
using Shared.Database;

namespace AuthServer;

public class AuthServerCommandHandler : ConsoleCommandHandler
{
    private readonly Dictionary<string, CommandHandler> serverCommands;
    private readonly Dictionary<string, CommandHandler> accountCoomands;
    private readonly Server _server;
    private readonly ILoginDatabase _loginDatabase;
    private static readonly CancellationTokenSource _cts = new();

    public AuthServerCommandHandler(Server server, ILoginDatabase loginDatabase) : base(_cts.Token)
    {
        _server = server;
        _loginDatabase = loginDatabase;
        Commands = new Dictionary<string, CommandHandler>()
        {
            { "account", HandleAccountCommand },
            { "server", HandleServerCommand },
        };

        serverCommands = new Dictionary<string, CommandHandler>()
        {
            { "start", HandleServerStartCommand },
            { "stop", HandleServerStopCommand },
            { "exit", HandleServerExitCommand },
        };

        accountCoomands = new Dictionary<string, CommandHandler>()
        {
            { "create", HandleAccountCreateCommand },
        };
    }

    private void HandleAccountCommand(ReadOnlySpan<string> args)
    {
        HandleSubCommand(accountCoomands, args, "account");
    }

    private void HandleAccountCreateCommand(ReadOnlySpan<string> args)
    {
        
    }

    private void HandleServerCommand(ReadOnlySpan<string> args)
    {
        HandleSubCommand(serverCommands, args, "server");
    }

    private void HandleServerStartCommand(ReadOnlySpan<string> __)
    {
        _ = _server.Start();
    }

    private void HandleServerStopCommand(ReadOnlySpan<string> _)
    {
        _server.Stop();
    }

    private void HandleServerExitCommand(ReadOnlySpan<string> _)
    {
        _cts.Cancel();
    }
}
