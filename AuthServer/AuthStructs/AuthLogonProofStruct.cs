using System.Runtime.InteropServices;

namespace AuthServer.AuthStructs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct AuthLogonProofStruct
{
    public fixed byte A[32];
    public fixed byte M1[20];
    public fixed byte crc[20];
    public byte number_of_keys;
    public byte securityFlags;
}