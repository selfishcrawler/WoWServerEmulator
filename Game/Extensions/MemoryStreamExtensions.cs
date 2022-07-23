using Game.Network;

namespace Game.Extensions;

public static class MemoryStreamExtensions
{
    public static void Write(this MemoryStream ms, in ServerPacketHeader header)
    {
        ms.Write(header.LengthBigEndian);
        ms.Write((ushort)header.Opcode);
    }
}