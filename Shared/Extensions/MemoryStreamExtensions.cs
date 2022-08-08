using System.Text;

namespace Shared.Extensions;

public static class MemoryStreamExtensions
{
    public static unsafe void Write<T>(this MemoryStream ms, T Data) where T : unmanaged, Enum
    {
        var bytes = new ReadOnlySpan<byte>(&Data, sizeof(T));
        ms.Write(bytes);
    }

    public static void Write(this MemoryStream ms, byte data)
    {
        ms.WriteByte(data);
    }

    public static void Write(this MemoryStream ms, ushort data)
    {
        Span<byte> dataBytes = stackalloc byte[sizeof(ushort)];
        BitConverter.TryWriteBytes(dataBytes, data);
        ms.Write(dataBytes);
    }

    public static void Write(this MemoryStream ms, uint data)
    {
        Span<byte> dataBytes = stackalloc byte[sizeof(uint)];
        BitConverter.TryWriteBytes(dataBytes, data);
        ms.Write(dataBytes);
    }

    public static void Write(this MemoryStream ms, ulong data)
    {
        Span<byte> dataBytes = stackalloc byte[sizeof(ulong)];
        BitConverter.TryWriteBytes(dataBytes, data);
        ms.Write(dataBytes);
    }

    public static void Write(this MemoryStream ms, float data)
    {
        Span<byte> dataBytes = stackalloc byte[sizeof(float)];
        BitConverter.TryWriteBytes(dataBytes, data);
        ms.Write(dataBytes);
    }

    public static void Write(this MemoryStream ms, bool data)
    {
        ms.WriteByte((byte)(data ? 1 : 0));
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
        ms.Position = 0;
    }
}