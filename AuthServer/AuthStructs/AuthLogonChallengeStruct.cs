using System.Runtime.InteropServices;

namespace AuthServer.AuthStructs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct AuthLogonChallengeStruct
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public ref struct IPStruct
    {
        [FieldOffset(0)]
        public uint Address;

        [FieldOffset(0)]
        public byte Octet1;
        [FieldOffset(1)]
        public byte Octet2;
        [FieldOffset(2)]
        public byte Octet3;
        [FieldOffset(3)]
        public byte Octet4;
    }

    public byte ProtocolVersion;
    public ushort Size;
    public fixed byte Gamename[4];
    public fixed byte Version[3];
    public ushort Build;
    public fixed byte Platform[4];
    public fixed byte OS[4];
    public fixed byte Locale[4];
    public uint Timezone_bias;
    public IPStruct IP;
    public byte AccountNameLength;
    public fixed byte AccountName[16];

    public static void Reverse(byte* array)
    {
        (array[0], array[1], array[2], array[3]) = (array[3], array[2], array[1], array[0]);
    }
}