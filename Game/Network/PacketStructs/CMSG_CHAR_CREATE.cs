using System.Runtime.InteropServices;
using Game.Entities;

namespace Game.Network.PacketStructs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct CMSG_CHAR_CREATE
{
    //public char* Name;
    public Race Race;
    public Class Class;
    public Gender Gender;
    public byte Skin;
    public byte Face;
    public byte HairStyle;
    public byte HairColor;
    public byte FacialStyle;
    public byte OutfitId;
}
