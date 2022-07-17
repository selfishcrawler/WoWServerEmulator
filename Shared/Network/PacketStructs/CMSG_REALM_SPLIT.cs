using System.Runtime.InteropServices;

namespace Shared.Network.PacketStructs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public ref struct CMSG_REALM_SPLIT
{
    public uint Unk;
}
