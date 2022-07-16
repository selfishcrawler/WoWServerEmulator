using System.Runtime.CompilerServices;
using Shared.Network;

namespace Shared.Extensions;

public static class MemoryStreamExtensions
{
    public static void Write<T>(this MemoryStream ms, T Data) where T : Enum
    {
        ms.WriteByte(Unsafe.As<T, byte>(ref Data));
    }

    public static void Write(this MemoryStream ms, byte data)
    {
        ms.WriteByte(data);
    }

    public static void Write(this MemoryStream ms, ushort data)
    {
        ms.Write(BitConverter.GetBytes(data));
    }

    public static void Write(this MemoryStream ms, uint data)
    {
        ms.Write(BitConverter.GetBytes(data));
    }

    public static void Write(this MemoryStream ms, ServerPacketHeader header)
    {
        ms.Write(header.LengthBigEndian);
        ms.Write((ushort)header.Opcode);
    }

    public static void Reset(this MemoryStream ms)
    {
        ms.Seek(0, SeekOrigin.Begin);
    }

}