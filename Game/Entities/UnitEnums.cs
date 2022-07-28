﻿namespace Game.Entities;

public enum Race : byte
{
    Human = 1,
    Orc,
    Dwarf,
    NightElf,
    Undead,
    Tauren,
    Gnome,
    Troll,
    Goblin,
    BloodElf,
    Draenei
}

public enum Class : byte
{
    Warrior = 1,
    Paladin,
    Hunter,
    Rogue,
    Priest,
    DeathKnight,
    Shaman,
    Mage,
    Warlock,
    Monk,
    Druid,
}

public enum Gender : byte
{
    Male,
    Female,
}

public enum PowerType : byte
{
    Mana,
    Rage,
    Focus,
    Energy,
    Happiness,
    Runes,
    RunicPower
}

public enum MovementFlags : uint
{
    MOVEMENTFLAG_NONE = 0x00000000,
    MOVEMENTFLAG_FORWARD = 0x00000001,
    MOVEMENTFLAG_BACKWARD = 0x00000002,
    MOVEMENTFLAG_STRAFE_LEFT = 0x00000004,
    MOVEMENTFLAG_STRAFE_RIGHT = 0x00000008,
    MOVEMENTFLAG_LEFT = 0x00000010,
    MOVEMENTFLAG_RIGHT = 0x00000020,
    MOVEMENTFLAG_PITCH_UP = 0x00000040,
    MOVEMENTFLAG_PITCH_DOWN = 0x00000080,
    MOVEMENTFLAG_WALKING = 0x00000100,
    MOVEMENTFLAG_ONTRANSPORT = 0x00000200,
    MOVEMENTFLAG_DISABLE_GRAVITY = 0x00000400,
    MOVEMENTFLAG_ROOT = 0x00000800,
    MOVEMENTFLAG_FALLING = 0x00001000,
    MOVEMENTFLAG_FALLING_FAR = 0x00002000,
    MOVEMENTFLAG_PENDING_STOP = 0x00004000,
    MOVEMENTFLAG_PENDING_STRAFE_STOP = 0x00008000,
    MOVEMENTFLAG_PENDING_FORWARD = 0x00010000,
    MOVEMENTFLAG_PENDING_BACKWARD = 0x00020000,
    MOVEMENTFLAG_PENDING_STRAFE_LEFT = 0x00040000,
    MOVEMENTFLAG_PENDING_STRAFE_RIGHT = 0x00080000,
    MOVEMENTFLAG_PENDING_ROOT = 0x00100000,
    MOVEMENTFLAG_SWIMMING = 0x00200000,
    MOVEMENTFLAG_ASCENDING = 0x00400000,
    MOVEMENTFLAG_DESCENDING = 0x00800000,
    MOVEMENTFLAG_CAN_FLY = 0x01000000,
    MOVEMENTFLAG_FLYING = 0x02000000,
    MOVEMENTFLAG_SPLINE_ELEVATION = 0x04000000,
    MOVEMENTFLAG_SPLINE_ENABLED = 0x08000000,
    MOVEMENTFLAG_WATERWALKING = 0x10000000,
    MOVEMENTFLAG_FALLING_SLOW = 0x20000000,
    MOVEMENTFLAG_HOVER = 0x40000000,
};

public enum MovementFlags2 : ushort
{
    MOVEMENTFLAG2_NONE = 0x00000000,
    MOVEMENTFLAG2_NO_STRAFE = 0x00000001,
    MOVEMENTFLAG2_NO_JUMPING = 0x00000002,
    MOVEMENTFLAG2_UNK3 = 0x00000004,
    MOVEMENTFLAG2_FULL_SPEED_TURNING = 0x00000008,
    MOVEMENTFLAG2_FULL_SPEED_PITCHING = 0x00000010,
    MOVEMENTFLAG2_ALWAYS_ALLOW_PITCHING = 0x00000020,
    MOVEMENTFLAG2_UNK7 = 0x00000040,
    MOVEMENTFLAG2_UNK8 = 0x00000080,
    MOVEMENTFLAG2_UNK9 = 0x00000100,
    MOVEMENTFLAG2_UNK10 = 0x00000200,
    MOVEMENTFLAG2_INTERPOLATED_MOVEMENT = 0x00000400,
    MOVEMENTFLAG2_INTERPOLATED_TURNING = 0x00000800,
    MOVEMENTFLAG2_INTERPOLATED_PITCHING = 0x00001000,
    MOVEMENTFLAG2_UNK14 = 0x00002000,
    MOVEMENTFLAG2_UNK15 = 0x00004000,
    MOVEMENTFLAG2_UNK16 = 0x00008000
};