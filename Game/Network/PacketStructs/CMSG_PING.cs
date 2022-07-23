using System.Runtime.InteropServices;

namespace Game.Network.PacketStructs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public ref struct CMSG_PING
{
    public uint Ping;
    public uint Latency;
}
