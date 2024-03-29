﻿using System.Runtime.InteropServices;

namespace Game.Network.PacketStructs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct CMSG_AUTH_SESSION
{
    public uint Build;
    public uint LoginServerID;

    public char* Username;

    public uint LoginServerType;
    public uint Seed;
    public uint RegionID;
    public uint BattlegroupID;
    public uint RealmID;
    public ulong DOS;
    public fixed byte Proof[20];
    public byte* AddonData;
}
