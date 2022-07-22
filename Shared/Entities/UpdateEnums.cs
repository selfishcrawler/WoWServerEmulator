namespace Shared.Entities;

public enum ObjectUpdateType : byte
{
    Values,
    Movement,
    CreateObject,
    CreateObject2,
    OutOfRangeObjects,
    NearObjects,
}

[Flags]
public enum ObjectUpdateFlag : ushort
{
    None                = 0x0000,
    Self                = 0x0001,
    Transport           = 0x0002,
    HasTarget           = 0x0004,
    Unknown             = 0x0008,
    LowGuid             = 0x0010,
    Living              = 0x0020,
    StationaryPosition  = 0x0040,
    Vehicle             = 0x0080,
    Position            = 0x0100,
    Rotation            = 0x0200
}

public enum ObjectType : byte
{
    Object,
    Item,
    Container,
    Unit,
    Player,
    GameObject,
    DynamicObject,
    Corpse
}

[Flags]
public enum TypeMask
{
    Object          = 0x0001,
    Item            = 0x0002,
    Container       = Item | 0x0004,
    Unit            = 0x0008,
    Player          = 0x0010,
    GameObject      = 0x0020,
    DynamicObject   = 0x0040,
    Corpse          = 0x0080,
    Seer            = Player | Unit | DynamicObject,
}

public enum HighGuid
{
    Item            = 0x4000,
    Container       = 0x4000,
    Player          = 0x0000,
    GameObject      = 0xF110,
    Transport       = 0xF120,
    Unit            = 0xF130,
    Pet             = 0xF140,
    Vehicle         = 0xF150,
    DynamicObject   = 0xF100,
    Corpse          = 0xF101,
    Mo_Transport    = 0x1FC0,
    Instance        = 0x1F40,
    Group           = 0x1F50
}