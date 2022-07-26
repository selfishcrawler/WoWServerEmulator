namespace Game.Entities;
using static EUnitFields;


public abstract class Unit : BaseEntity
{
    protected uint _currentHealth, _maxHealth, _level, _displayID, _nativeDisplayID;
    protected PowerType _powerType;

    public bool Alive { get; set; }

    public uint CurrentHealth
    {
        get => _currentHealth;
        set
        {
            _currentHealth = value;
            SetField(UNIT_FIELD_HEALTH, value);
        }
    }

    public uint MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            SetField(UNIT_FIELD_MAXHEALTH, value);
        }
    }

    public uint Level
    {
        get => _level;
        set
        {
            _level = value;
            SetField(UNIT_FIELD_LEVEL, value);
        }
    }

    public uint DisplayID
    {
        get => _displayID;
        set
        {
            _displayID = value;
            SetField(UNIT_FIELD_DISPLAYID, value);
        }
    }

    public uint NativeDisplayID
    {
        get => _nativeDisplayID;
        set
        {
            _nativeDisplayID = value;
            SetField(UNIT_FIELD_NATIVEDISPLAYID, value);
        }
    }

    public Race Race { get; init; }

    public Class Class { get; init; }

    public Gender Gender { get; init; }

    public unsafe PowerType PowerType
    {
        get => _powerType;
        set
        {
            _powerType = value;
            uint unitBytes = 0;
            Span<byte> cast = new Span<byte>(&unitBytes, 4);
            cast[0] = (byte)Race;
            cast[1] = (byte)Class;
            cast[2] = (byte)Gender;
            cast[3] = (byte)value;
            SetField(UNIT_FIELD_BYTES_0, unitBytes);
        }
    }

    public float WalkSpeed { get; set; }
    public float RunSpeed { get; set; }
    public float BackwardsRunSpeed { get; set; }
    public float SwimSpeed { get; set; }
    public float BackwardsSwimSpeed { get; set; }
    public float FlySpeed { get; set; }
    public float BackwardsFlySpeed { get; set; }
    public float TurnRate { get; set; }
    public float PitchRate { get; set; }

    protected Unit(uint guid, int bitCount) : base(guid, bitCount)
    {
        TurnRate = MathF.PI;
    }

    protected override void BuildUpdatePacket(ObjectUpdateType updateType, MemoryStream ms)
    {
        base.BuildUpdatePacket(updateType, ms);
    }

    protected void BuildMovementBlock(MemoryStream ms)
    {
        ms.Write((uint)0); //movement flags
        ms.Write((ushort)0); //extra movement flags
        ms.Write((uint)DateTimeOffset.Now.ToUnixTimeSeconds()); //timestamp
        ms.Write(Position.X);
        ms.Write(Position.Y);
        ms.Write(Position.Z);
        ms.Write(Position.Orientation);
        ms.Write((uint)1); //fall time

        ms.Write(WalkSpeed);
        ms.Write(RunSpeed);
        ms.Write(BackwardsRunSpeed);
        ms.Write(SwimSpeed);
        ms.Write(BackwardsSwimSpeed);
        ms.Write(FlySpeed);
        ms.Write(BackwardsFlySpeed);
        ms.Write(TurnRate);
        ms.Write(PitchRate);
    }

    public void SetCurrentPower(PowerType type, uint value)
    {
        uint powerField = (uint)UNIT_FIELD_POWER1 + (uint)type;
        SetField((EUnitFields)powerField, value);
    }
    public void SetMaxPower(PowerType type, uint value)
    {
        uint powerField = (uint)UNIT_FIELD_MAXPOWER1 + (uint)type;
        SetField((EUnitFields)powerField, value);
    }
}
