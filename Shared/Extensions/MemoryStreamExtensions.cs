using System.Runtime.CompilerServices;
using System.Text;
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

    public static void Write(this MemoryStream ms, ulong data)
    {
        ms.Write(BitConverter.GetBytes(data));
    }

    public static void Write(this MemoryStream ms, float data)
    {
        ms.Write(BitConverter.GetBytes(data));
    }

    public static void Write(this MemoryStream ms, in ServerPacketHeader header)
    {
        ms.Write(header.LengthBigEndian);
        ms.Write((ushort)header.Opcode);
    }

    public static void Write(this MemoryStream ms, string data, bool terminated = true)
    {
        int len = Encoding.UTF8.GetByteCount(data);
        Span<byte> buf = stackalloc byte[len + (terminated ? 1 : 0)];
        Encoding.UTF8.GetBytes(data, buf);
        if (terminated)
            buf[buf.Length - 1] = 0;
        ms.Write(buf);
    }

    public static void Reset(this MemoryStream ms)
    {
        ms.Seek(0, SeekOrigin.Begin);
    }

}