using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Game.Network.Clustering;

[JsonDerivedType(typeof(EnterWorldPacket), 0)]
public class ClusterPacket
{
    private static object lockObject = new();

    public void Send(NetworkStream stream)
    {
        using MemoryStream buffer = new(100);
        JsonSerializer.Serialize(buffer, this);
        lock (lockObject)
        {
            stream.Write(BitConverter.GetBytes((int)buffer.Length));
            buffer.Reset();
            buffer.CopyTo(stream);
        }
    }

    public static async Task<ClusterPacket> ReceiveAsync(NetworkStream stream)
    {
        byte[] length = new byte[4];
        await stream.ReadExactlyAsync(length, 0, 4);
        int count = BitConverter.ToInt32(length);
        byte[] buffer = new byte[count];
        await stream.ReadExactlyAsync(buffer, 0, count);
        return Deserialize(buffer);

        ClusterPacket Deserialize(byte[] buf)
        {
            Utf8JsonReader reader = new(buf);
            return JsonSerializer.Deserialize<ClusterPacket>(ref reader);
        }
    }
}

public sealed class EnterWorldPacket : ClusterPacket
{
    public int AccountId { get; set; }
    public uint CharacterId { get; set; }
}