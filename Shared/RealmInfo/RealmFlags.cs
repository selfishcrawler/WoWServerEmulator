namespace Shared.RealmInfo;

[Flags]
public enum RealmFlags : byte
{
    NONE = 0x00,
    INVALID = 0x01,
    OFFLINE = 0x02,
    SPECIFY_BUILD = 0x04,
    UNK_1 = 0x08,
    UNK_2 = 0x0F,
    NEW_PLAYERS = 0x10,
    RECOMMENDED = 0x20,
    FULL = 0x40,
}