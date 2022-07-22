namespace Shared.Network;

public enum Opcode : uint
{
    CMSG_CHAR_CREATE                            = 0x036,
    CMSG_CHAR_ENUM                              = 0x037,

    SMSG_CHAR_CREATE                            = 0x03A,
    SMSG_CHAR_ENUM                              = 0x03B,
    SMSG_CHAR_DELETE                            = 0x03C,
    CMSG_PLAYER_LOGIN                           = 0x03D,

    SMSG_LOGIN_SET_TIME_SPEED                   = 0x042, //

    CMSG_NAME_QUERY                             = 0x050,
    SMSG_NAME_QUERY_RESPONSE                    = 0x051,

    SMSG_UPDATE_OBJECT                          = 0x0A9,

    SMSG_TUTORIAL_FLAGS                         = 0x0FD,

    CMSG_PING                                   = 0x1DC,
    SMSG_PONG                                   = 0x1DD,

    SMSG_AUTH_CHALLENGE                         = 0x1EC,
    CMSG_AUTH_SESSION                           = 0x1ED,
    SMSG_AUTH_RESPONSE                          = 0x1EE,

    SMSG_COMPRESSED_UPDATE_OBJECT               = 0x1F6,

    SMSG_ACCOUNT_DATA_TIMES                     = 0x209,

    SMSG_LOGIN_VERIFY_WORLD                     = 0x236,

    CMSG_SET_ACTIVE_MOVER                       = 0x26A,

    SMSG_REALM_SPLIT                            = 0x38B,
    CMSG_REALM_SPLIT                            = 0x38C,

    SMSG_TIME_SYNC_REQ                          = 0x390,
    CMSG_TIME_SYNC_RESP                         = 0x391,

    SMSG_FEATURE_SYSTEM_STATUS                  = 0x3C9,

    CMSG_SET_ACTIVE_VOICE_CHANNEL               = 0x3D3,

    CMSG_SET_PLAYER_DECLINED_NAMES              = 0x419,
    SMSG_SET_PLAYER_DECLINED_NAMES_RESULT       = 0x41A,

    CMSG_READY_FOR_ACCOUNT_DATA_TIMES           = 0x4FF,

    SMSG_REDIRECT_CLIENT                        = 0x50D,
    CMSG_REDIRECTION_FAILED                     = 0x50E,
    CMSG_REDIRECTION_AUTH_PROOF                 = 0x512,
}