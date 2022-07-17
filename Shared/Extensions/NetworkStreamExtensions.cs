using Shared.Network;
using System.Net.Sockets;

namespace Shared.Extensions;

public static class NetworkStreamExtensions
{
    public static async Task PushPacketAsync(this NetworkStream ns, MemoryStream ms, CancellationToken token = default)
    {
        await ns.WriteAsync(ms.GetBuffer(), 0, (int)ms.Position, token).ConfigureAwait(false);
    }

    public static void PushPacket(this NetworkStream ns, MemoryStream ms)
    {
        ns.Write(ms.GetBuffer(), 0, (int)ms.Position);
        ms.Reset();
    }
    public static void Write(this NetworkStream ns, in ServerPacketHeader header)
    {
        ns.Write(BitConverter.GetBytes(header.LengthBigEndian));
        ns.Write(BitConverter.GetBytes((ushort)header.Opcode));
    }
}