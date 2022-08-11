using Game.Prototypes;

namespace Game.Entities;
using static EItemFields;

public class Item : BaseEntity
{
    private Unit _owner;
    private uint _durability, _maxDurability, _stackCount;
    private Player _creator;

    public override uint HighGuid => (uint)Entities.HighGuid.Item;
    protected override ObjectType ObjectType => ObjectType.Item;
    protected override TypeMask TypeMask => TypeMask.Object | TypeMask.Item;

    public new required ItemPrototype Prototype { get => (ItemPrototype)base.Prototype; init => base.Prototype = value; }
    public override string Name => Prototype.Name;
    public required Unit Owner
    {
        get => _owner;
        set
        {
            _owner = value;
            SetField(ITEM_FIELD_OWNER, _owner.Guid);
            SetField(ITEM_FIELD_OWNER + 1, _owner.HighGuid);
        }
    }

    public Player Creator
    {
        get => _creator;
        set
        {
            _creator = value;
            SetField(ITEM_FIELD_CREATOR, _creator.Guid);
            SetField(ITEM_FIELD_CREATOR + 1, _creator.HighGuid);
        }
    }

    public uint Durability
    {
        get => _durability;
        set
        {
            _durability = value;
            SetField(ITEM_FIELD_DURABILITY, value);
        }
    }

    public uint MaxDurability
    {
        get => _maxDurability;
        set
        {
            _maxDurability = value;
            SetField(ITEM_FIELD_MAXDURABILITY, value);
        }
    }

    public uint StackCount
    {
        get => _stackCount;
        set
        {
            _stackCount = value;
            SetField(ITEM_FIELD_STACK_COUNT, value);
        }
    }

    public Item() : base((int)ITEM_END)
    {
    }

    public void BuildCreatePacket(MemoryStream ms)
    {
        BuildPacket(ObjectUpdateType.CreateObject, ms);
        ms.Write(ObjectUpdateFlag.None);
        WriteFullTable(ms);
    }
}