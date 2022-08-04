namespace Game.Entities;
using static EItemFields;

public class Item : BaseEntity
{
    protected Unit _owner;

    //private static readonly byte[] _highGuidBytes = BitConverter.GetBytes((uint)Entities.HighGuid.Item);
    public override ReadOnlySpan<byte> HighGuid => ReadOnlySpan<byte>.Empty;//_highGuidBytes;
    protected override ObjectType ObjectType => ObjectType.Item;
    protected override TypeMask TypeMask => TypeMask.Object | TypeMask.Item;

    public override required uint Entry { get => base.Entry; init => base.Entry = value; }
    public Unit Owner
    {
        get => _owner;
        set
        {
            _owner = value;
            SetField(ITEM_FIELD_OWNER, BitConverter.ToUInt32(_owner.LowGuid));
            SetField(ITEM_FIELD_OWNER + 1, BitConverter.ToUInt32(_owner.HighGuid));
        }
    }

    public required uint DisplayID { get; init; }
    public required ItemClass ItemClass { get; init; }
    public required ItemSubclass ItemSubclass { get; init; }

    public Item() : base((int)ITEM_END) // don't need guid for items?
    {
    }
}