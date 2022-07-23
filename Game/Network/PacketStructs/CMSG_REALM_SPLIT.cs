using System.Runtime.InteropServices;

namespace Game.Network.PacketStructs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public ref struct CMSG_REALM_SPLIT
{
    public uint Unk;
}
