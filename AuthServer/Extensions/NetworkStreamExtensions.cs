using System.Net.Sockets;

namespace AuthServer.Extensions;

public static class NetworkStreamExtensions
{
    public static async Task PushPacketAsync(this NetworkStream ns, MemoryStream ms, CancellationToken token = default)
    {
        await ns.WriteAsync(ms.GetBuffer(), 0, (int)ms.Position, token).ConfigureAwait(false);
    }

    public static void PushPacket(this NetworkStream ns, MemoryStream ms)
    {
        ns.Write(ms.GetBuffer(), 0, (int)ms.Position);
    }
}