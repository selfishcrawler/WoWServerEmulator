namespace Game.Network;
using static Opcode;

public enum Opcode : uint
{
    CMSG_CHAR_CREATE                            = 0x036,
    CMSG_CHAR_ENUM                              = 0x037,
    CMSG_CHAR_DELETE                            = 0x038,
    //SMSG_AUTH_SRP6_RESPONSE 0x039
    SMSG_CHAR_CREATE                            = 0x03A,
    SMSG_CHAR_ENUM                              = 0x03B,
    SMSG_CHAR_DELETE                            = 0x03C,
    CMSG_PLAYER_LOGIN                           = 0x03D,

    SMSG_CHARACTER_LOGIN_FAILED                 = 0x041,
    SMSG_LOGIN_SET_TIME_SPEED                   = 0x042, //

    CMSG_LOGOUT_REQUEST                         = 0x04B,
    SMSG_LOGOUT_RESPONSE                        = 0x04C,
    SMSG_LOGOUT_COMPLETE                        = 0x04D,

    CMSG_NAME_QUERY                             = 0x050,
    SMSG_NAME_QUERY_RESPONSE                    = 0x051,

    CMSG_ITEM_QUERY_SINGLE                      = 0x056,
    CMSG_ITEM_QUERY_MULTIPLE                    = 0x057,
    SMSG_ITEM_QUERY_SINGLE_RESPONSE             = 0x058,
    SMSG_ITEM_QUERY_MULTIPLE_RESPONSE           = 0x059,

    CMSG_CREATURE_QUERY                         = 0x060,
    SMSG_CREATURE_QUERY_RESPONSE                = 0x061,

    SMSG_UPDATE_OBJECT                          = 0x0A9,

    MSG_MOVE_START_FORWARD                      = 0x0B5,
    MSG_MOVE_START_BACKWARD                     = 0x0B6,
    MSG_MOVE_STOP                               = 0x0B7,
    MSG_MOVE_START_STRAFE_LEFT                  = 0x0B8,
    MSG_MOVE_START_STRAFE_RIGHT                 = 0x0B9,
    MSG_MOVE_STOP_STRAFE                        = 0x0BA,
    MSG_MOVE_JUMP                               = 0x0BB,
    MSG_MOVE_START_TURN_LEFT                    = 0x0BC,
    MSG_MOVE_START_TURN_RIGHT                   = 0x0BD,
    MSG_MOVE_STOP_TURN                          = 0x0BE,
    MSG_MOVE_START_PITCH_UP                     = 0x0BF,
    MSG_MOVE_START_PITCH_DOWN                   = 0x0C0,
    MSG_MOVE_STOP_PITCH                         = 0x0C1,
    MSG_MOVE_SET_RUN_MODE                       = 0x0C2,
    MSG_MOVE_SET_WALK_MODE                      = 0x0C3,
    MSG_MOVE_TOGGLE_LOGGING                     = 0x0C4,
    MSG_MOVE_TELEPORT                           = 0x0C5,
    MSG_MOVE_TELEPORT_CHEAT                     = 0x0C6,
    MSG_MOVE_TELEPORT_ACK                       = 0x0C7,
    MSG_MOVE_TOGGLE_FALL_LOGGING                = 0x0C8,
    MSG_MOVE_FALL_LAND                          = 0x0C9,
    MSG_MOVE_START_SWIM                         = 0x0CA,
    MSG_MOVE_STOP_SWIM                          = 0x0CB,
    MSG_MOVE_SET_RUN_SPEED_CHEAT                = 0x0CC,
    MSG_MOVE_SET_RUN_SPEED                      = 0x0CD,
    MSG_MOVE_SET_RUN_BACK_SPEED_CHEAT           = 0x0CE,
    MSG_MOVE_SET_RUN_BACK_SPEED                 = 0x0CF,
    MSG_MOVE_SET_WALK_SPEED_CHEAT               = 0x0D0,
    MSG_MOVE_SET_WALK_SPEED                     = 0x0D1,
    MSG_MOVE_SET_SWIM_SPEED_CHEAT               = 0x0D2,
    MSG_MOVE_SET_SWIM_SPEED                     = 0x0D3,
    MSG_MOVE_SET_SWIM_BACK_SPEED_CHEAT          = 0x0D4,
    MSG_MOVE_SET_SWIM_BACK_SPEED                = 0x0D5,
    MSG_MOVE_SET_ALL_SPEED_CHEAT                = 0x0D6,
    MSG_MOVE_SET_TURN_RATE_CHEAT                = 0x0D7,
    MSG_MOVE_SET_TURN_RATE                      = 0x0D8,
    MSG_MOVE_TOGGLE_COLLISION_CHEAT             = 0x0D9,
    MSG_MOVE_SET_FACING                         = 0x0DA,
    MSG_MOVE_SET_PITCH                          = 0x0DB,
    MSG_MOVE_WORLDPORT_ACK                      = 0x0DC,

    MSG_MOVE_HEARTBEAT                          = 0x0EE,

    SMSG_TUTORIAL_FLAGS                         = 0x0FD,

    CMSG_PING                                   = 0x1DC,
    SMSG_PONG                                   = 0x1DD,

    SMSG_AUTH_CHALLENGE                         = 0x1EC,
    CMSG_AUTH_SESSION                           = 0x1ED,
    SMSG_AUTH_RESPONSE                          = 0x1EE,

    SMSG_COMPRESSED_UPDATE_OBJECT               = 0x1F6,

    SMSG_ACCOUNT_DATA_TIMES                     = 0x209,
    CMSG_REQUEST_ACCOUNT_DATA                   = 0x20A,
    CMSG_UPDATE_ACCOUNT_DATA                    = 0x20B,
    SMSG_UPDATE_ACCOUNT_DATA                    = 0x20C,

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
    SMSG_SUSPEND_COMMS                          = 0x50F,
    CMSG_SUSPEND_COMMS_ACK                      = 0x510,
    SMSG_RESUME_COMMS                           = 0x511,
    CMSG_REDIRECTION_AUTH_PROOF                 = 0x512,
}

public partial class WorldSession
{
    private PacketHandler GetHandlerForPacket(ClientPacketHeader header) => header.Opcode switch
    {
        CMSG_CHAR_CREATE => HandleCharCreate,
        CMSG_CHAR_ENUM => HandleCharEnum,
        CMSG_CHAR_DELETE => HandleCharDelete,
        CMSG_PLAYER_LOGIN => HandlePlayerLogin,
        CMSG_LOGOUT_REQUEST => HandleLogoutRequest,
        CMSG_NAME_QUERY => HandleNameQuery,
        CMSG_ITEM_QUERY_SINGLE => HandleItemQuerySingle,

        MSG_MOVE_START_FORWARD => HandleMovementPacket,
        MSG_MOVE_START_BACKWARD => HandleMovementPacket,
        MSG_MOVE_STOP => HandleMovementPacket,
        MSG_MOVE_START_STRAFE_LEFT => HandleMovementPacket,
        MSG_MOVE_START_STRAFE_RIGHT => HandleMovementPacket,
        MSG_MOVE_STOP_STRAFE => HandleMovementPacket,
        MSG_MOVE_JUMP => HandleMovementPacket,
        MSG_MOVE_START_TURN_LEFT => HandleMovementPacket,
        MSG_MOVE_START_TURN_RIGHT => HandleMovementPacket,
        MSG_MOVE_STOP_TURN => HandleMovementPacket,
        MSG_MOVE_START_PITCH_UP => HandleMovementPacket,
        MSG_MOVE_START_PITCH_DOWN => HandleMovementPacket,
        MSG_MOVE_STOP_PITCH => HandleMovementPacket,
        MSG_MOVE_SET_RUN_MODE => HandleMovementPacket,
        MSG_MOVE_SET_WALK_MODE => HandleMovementPacket,
        MSG_MOVE_TOGGLE_LOGGING => HandleMovementPacket,
        MSG_MOVE_TELEPORT => HandleMovementPacket,
        MSG_MOVE_TELEPORT_ACK => HandleMovementPacket,
        MSG_MOVE_TOGGLE_FALL_LOGGING => HandleMovementPacket,
        MSG_MOVE_FALL_LAND => HandleMovementPacket,
        MSG_MOVE_START_SWIM => HandleMovementPacket,
        MSG_MOVE_STOP_SWIM => HandleMovementPacket,
        MSG_MOVE_SET_RUN_SPEED => HandleMovementPacket,
        MSG_MOVE_SET_RUN_BACK_SPEED => HandleMovementPacket,
        MSG_MOVE_SET_WALK_SPEED => HandleMovementPacket,
        MSG_MOVE_SET_SWIM_SPEED => HandleMovementPacket,
        MSG_MOVE_SET_SWIM_BACK_SPEED => HandleMovementPacket,
        MSG_MOVE_SET_TURN_RATE => HandleMovementPacket,
        MSG_MOVE_SET_FACING => HandleMovementPacket,
        MSG_MOVE_SET_PITCH => HandleMovementPacket,
        MSG_MOVE_WORLDPORT_ACK => HandleMovementPacket,

        MSG_MOVE_HEARTBEAT => HandleMovementPacket,

        CMSG_PING => HandlePing,
        CMSG_REQUEST_ACCOUNT_DATA => HandleRequestAccountData,
        CMSG_UPDATE_ACCOUNT_DATA => HandleUpdateAccountData,
        CMSG_SET_ACTIVE_MOVER => HandleSetActiveMover,
        CMSG_REALM_SPLIT => HandleRealmSplit,
        CMSG_TIME_SYNC_RESP => HandleTimeSyncResponce,
        CMSG_SET_PLAYER_DECLINED_NAMES => HandleSetPlayerDeclinedNames,
        CMSG_READY_FOR_ACCOUNT_DATA_TIMES => HandleReadyForAccountDataTimes,

        CMSG_REDIRECTION_FAILED => HandleRedirectionFailed,
        _ => UnhandledPacket,
    };
}