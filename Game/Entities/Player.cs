namespace Game.Entities;
using static EUnitFields;

public sealed class Player : Unit
{
    private static readonly byte[] _highGuidBytes = BitConverter.GetBytes((uint)Entities.HighGuid.Player);
    public override ReadOnlySpan<byte> HighGuid => _highGuidBytes;
    protected override ObjectType ObjectType => ObjectType.Player;
    protected override TypeMask TypeMask => TypeMask.Player | TypeMask.Unit | TypeMask.Object;

    public override uint Entry => 0;
    public Item[] Equipment { get; init; }

    public Player(uint guid) : base(guid, (int)PLAYER_END)
    {
        Equipment = new Item[(int)EquipmentSlot.EQUIPMENT_SLOT_COUNT];
    }

    public void BuildCreatePacket(MemoryStream ms, bool self = true)
    {
        BuildPacket(ObjectUpdateType.CreateObject2, ms);
        ObjectUpdateFlag updateFlags = ObjectUpdateFlag.None;
        if (Alive)
            updateFlags |= ObjectUpdateFlag.Living;
        if (self)
            updateFlags |= ObjectUpdateFlag.Self;
        ms.Write(updateFlags);

        BuildMovementBlock(ms);
        WriteFullTable(ms);
    }
}
