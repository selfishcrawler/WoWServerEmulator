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
    public override required Race Race
    {
        get => base.Race;
        init
        {
            base.Race = value;
            Faction = (uint)(Race switch
            {
                Race.Human => PlayerFaction.Human,
                Race.Orc => PlayerFaction.Orc,
                Race.Dwarf => PlayerFaction.Dwarf,
                Race.NightElf => PlayerFaction.NightElf,
                Race.Undead => PlayerFaction.Undead,
                Race.Tauren => PlayerFaction.Tauren,
                Race.Gnome => PlayerFaction.Gnome,
                Race.Troll => PlayerFaction.Troll,
                Race.BloodElf => PlayerFaction.BloodElf,
                Race.Draenei => PlayerFaction.Draenei,
                _ => (PlayerFaction)0,
            });
        }
    }

    public Player() : base((int)PLAYER_END)
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