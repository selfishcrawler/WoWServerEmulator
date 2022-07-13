using System.Runtime.InteropServices;

namespace AuthServer.AuthStructs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe ref struct AuthReconnectProofStruct
{
    public fixed byte ClientData[16];
    public fixed byte Proof[20];
    public fixed byte Checksum[20];
    public byte number_of_keys;
}