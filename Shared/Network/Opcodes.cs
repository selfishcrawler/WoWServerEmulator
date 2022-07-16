namespace Shared.Network;

public enum Opcode : uint
{
    SMSG_AUTH_CHALLENGE = 0x1EC,
    CMSG_AUTH_SESSION = 0x1ED,
}