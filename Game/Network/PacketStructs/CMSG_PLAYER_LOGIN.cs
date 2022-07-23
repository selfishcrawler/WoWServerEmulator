using System.Runtime.InteropServices;

namespace Game.Network.PacketStructs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct CMSG_PLAYER_LOGIN
{
    public ulong Guid;
}
