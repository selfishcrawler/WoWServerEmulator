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
        if (args.Length != 2)
        {
            Log.Error("Wrong argument count: provide username and password!");
            return;
        }
        if (args[0].Length > 16)
        {
            Log.Error("Username cannot be longer than 16 chars");
            return;
        }
        var salt = SRP6.GenerateSalt().ToArray();
        var verifier = SRP6.CalculateVerifier(args[0], args[1], salt);
        try
        {
            _loginDatabase.ExecuteNonQuery(_loginDatabase.CreateAccount, new KeyValuePair<string, object>[]
            {
            new("@Username", args[0].ToUpper()),
            new("@Verifier", verifier.ToArray()),
            new("@Salt", salt),
            });
        }
        catch (Exception ex)
        {
            Log.Error($"Cannot create account with values: {ex.Message}");
            return;
        }
        Log.Message("Account created successfully");
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
