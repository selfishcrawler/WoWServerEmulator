namespace Shared.Network;

public enum Opcode : uint
{
    CMSG_CHAR_ENUM                              = 0x037,
    SMSG_CHAR_ENUM                              = 0x03B,
    CMSG_PING                                   = 0x1DC,
    SMSG_PONG                                   = 0x1DD,
    SMSG_AUTH_CHALLENGE                         = 0x1EC,
    CMSG_AUTH_SESSION                           = 0x1ED,
    SMSG_AUTH_RESPONSE                          = 0x1EE,
    SMSG_REALM_SPLIT                            = 0x38B,
    CMSG_REALM_SPLIT                            = 0x38C,
    CMSG_READY_FOR_ACCOUNT_DATA_TIMES           = 0x4FF,
}