namespace Shared.Entities;
using static EUnitFields;

public abstract class Unit : BaseEntity
{
    protected float _x, _y, _z, _o;
    protected uint _currentHealth, _maxHealth, _level, _displayID, _nativeDisplayID;
    
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

    protected Unit(uint guid, int bitCount) : base(guid, bitCount)
    {
    }
}
