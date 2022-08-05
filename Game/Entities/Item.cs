namespace Game.Entities;
using static EItemFields;

public class Item : BaseEntity
{
    protected Unit _owner;

    public override uint HighGuid => (uint)Entities.HighGuid.Item;
    protected override ObjectType ObjectType => ObjectType.Item;
    protected override TypeMask TypeMask => TypeMask.Object | TypeMask.Item;

    public override required uint Entry { get => base.Entry; init => base.Entry = value; }
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

    public required uint DisplayID { get; init; }
    public required ItemClass ItemClass { get; init; }
    public required ItemSubclass ItemSubclass { get; init; }

    public Item() : base((int)ITEM_END)
    {
    }
}