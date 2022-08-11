namespace Game.Entities;

using Game.Prototypes;
using Game.World;
using static EUnitFields;

public sealed class Player : Unit
{
    public override uint HighGuid => (uint)Entities.HighGuid.Player;
    protected override ObjectType ObjectType => ObjectType.Player;
    protected override TypeMask TypeMask => TypeMask.Player | TypeMask.Unit | TypeMask.Object;

    public override Prototype Prototype => null;
    public Item[] Equipment { get; private init; }
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
        Equipment[(int)EquipmentSlot.Head] = new Item()
        {
            Guid = 1,
            Prototype = WorldManager.GetItemProtoByEntry(34243),
            Owner = this,
            Durability = 100,
            MaxDurability = 100,
            StackCount = 1,
        };
        SetField(PLAYER_VISIBLE_ITEM_1_ENTRYID, 34243);
        SetField(PLAYER_VISIBLE_ITEM_1_ENCHANTMENT, 0);
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