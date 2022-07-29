namespace Game.Entities;
using static EItemFields;

public class Item : BaseEntity
{
    //private static readonly byte[] _highGuidBytes = BitConverter.GetBytes((uint)Entities.HighGuid.Item);
    public override ReadOnlySpan<byte> HighGuid => ReadOnlySpan<byte>.Empty;//_highGuidBytes;
    protected override ObjectType ObjectType => ObjectType.Item;
    protected override TypeMask TypeMask => TypeMask.Object | TypeMask.Item;

    public Item() : base(0, (int)ITEM_END) // don't need guid for items?
    {
    }
}