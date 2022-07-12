namespace AuthServer.Enums;

public enum AuthCommand
{
    AUTH_LOGON_CHALLENGE        = 0x00,
    AUTH_LOGON_PROOF            = 0x01,
    AUTH_RECONNECT_CHALLENGE    = 0x02,
    AUTH_RECONNECT_PROOF        = 0x03,

    REALMLIST                   = 0x10,
}