using Game.Network;
using System.Net.Sockets;

namespace Game.Extensions;

public static class NetworkStreamExtensions
{
    public static void Write(this NetworkStream ns, in ServerPacketHeader header)
    {
        ns.Write(BitConverter.GetBytes(header.LengthBigEndian));
        ns.Write(BitConverter.GetBytes((ushort)header.Opcode));
    }
}