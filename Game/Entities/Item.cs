using Game.Prototypes;

namespace Game.Entities;
using static EItemFields;

public class Item : BaseEntity
{
    protected Unit _owner;

    public override uint HighGuid => (uint)Entities.HighGuid.Item;
    protected override ObjectType ObjectType => ObjectType.Item;
    protected override TypeMask TypeMask => TypeMask.Object | TypeMask.Item;

    public new required ItemPrototype Prototype { get => (ItemPrototype)base.Prototype; init => base.Prototype = value; }
    public override string Name => Prototype.Name;
    public Unit Owner
    {
        get => _owner;
        set
        {
            _owner = value;
            SetField(ITEM_FIELD_OWNER, _owner.Guid);
            SetField(ITEM_FIELD_OWNER + 1, _owner.HighGuid);
        }
    }

    public Item() : base((int)ITEM_END)
    {
    }
}