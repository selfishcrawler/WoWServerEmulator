using Game.Network;
using System.Net.Sockets;

namespace Game.Extensions;

public static class NetworkStreamExtensions
{
    public static void Write(this NetworkStream ns, in ServerPacketHeader header)
    {
        Span<byte> headerBytes = stackalloc byte[2*sizeof(ushort)];
        BitConverter.TryWriteBytes(headerBytes, header.LengthBigEndian);
        BitConverter.TryWriteBytes(headerBytes[2..], (ushort)header.Opcode);
        ns.Write(headerBytes);
    }
}