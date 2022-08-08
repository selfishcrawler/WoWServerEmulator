using Game.Entities;

namespace Game.Prototypes;

public struct ItemStat
{
    public required uint Type;
    public required int Value;
}

public struct Damage
{
    public required float Min;
    public required float Max;
    public required DamageType Type;
}

public struct ItemSpell
{
    public required int ID;
    public required uint Trigger;
    public required int Charges;
    public required float PPMRate;
    public required int Cooldown;
    public required uint SpellCategory;
    public required int SpellCategoryCooldown;
}

public struct ItemSocket
{
    public required uint Color;
    public required uint Content;
}

public class ItemPrototype : Prototype
{
    private const uint MaxStats = 10;
    private const uint MaxDamages = 2;
    private const uint MaxSpells = 5;
    private const uint MaxSockets = 3;
    private readonly int MaxResistances = Enum.GetNames<DamageType>().Length;

    protected override void WritePrototypeValues(MemoryStream ms)
    {
        ms.Write(Entry);
        ms.Write(ItemClass);
        ms.Write(ItemSubclass);
        ms.Write(SoundOverrideSubclass);
        ms.Write(Name);
        ms.Write(0);
        ms.Write(0);
        ms.Write(0);
        ms.Write(DisplayID);
        ms.Write(Quality);
        ms.Write(Flags);
        ms.Write(Flags2);
        ms.Write(BuyPrice);
        ms.Write(SellPrice);
        ms.Write(InventoryType);
        ms.Write(AllowableClass);
        ms.Write(AllowableRace);
        ms.Write(ItemLevel);
        ms.Write(RequiredLevel);
        ms.Write(RequiredSkill);
        ms.Write(RequiredSkillRank);
        ms.Write(RequiredSpell);
        ms.Write(RequiredHonorRank);
        ms.Write(RequiredReputationFaction);
        ms.Write(RequiredReputationRank);
        ms.Write(MaxCount);
        ms.Write(BitConverter.GetBytes(Stackable));
        ms.Write(ContainerSlots);
        ms.Write(StatsCount);
        for (int i = 0; i < StatsCount; i++)
        {
            ms.Write(ItemStats[i].Type);
            ms.Write(BitConverter.GetBytes(ItemStats[i].Value));
        }
        ms.Write(ScalingStatDistribution);
        ms.Write(ScalingStatValue);
        for (int i = 0; i < MaxDamages; i++)
        {
            ms.Write(Damages[i].Min);
            ms.Write(Damages[i].Max);
            ms.Write(Damages[i].Type);
        }

        for (int i = 0; i < MaxResistances; i++)
        {
            ms.Write(Resistances[i]);
        }

        for (int i = 0; i < MaxSpells; i++)
        {
            //Todo: spells for items
            ms.Write(0);
            ms.Write(0);
            ms.Write(0);
            ms.Write(BitConverter.GetBytes(-1));
            ms.Write(0);
            ms.Write(BitConverter.GetBytes(-1));
        }

        ms.Write(Bonding);
        ms.Write(Description);
        ms.Write(PageText);
        ms.Write(Language);
        ms.Write(PageMaterial);
        ms.Write(StartQuest);
        ms.Write(LockID);
        ms.Write(BitConverter.GetBytes(Material));
        ms.Write(Sheath);
        ms.Write(RandomProperty);
        ms.Write(RandomSuffix);
        ms.Write(Block);
        ms.Write(ItemSet);
        ms.Write(MaxDurability);
        ms.Write(Area);
        ms.Write(Map);
        ms.Write(BagFamily);
        ms.Write(TotemCategory);
        for (int i = 0; i < MaxSockets; i++)
        {
            ms.Write(Sockets[i].Color);
            ms.Write(Sockets[i].Content);
        }
        ms.Write(SocketBonus);
        ms.Write(GemProperties);
        ms.Write(RequiredDisenchantSkill);
        ms.Write(ArmorDamageModifier);
        ms.Write(Duration);
        ms.Write(ItemLimitCategory);
        ms.Write(HolidayID);
    }

    public required ItemClass ItemClass { get; init; }
    public required ItemSubclass ItemSubclass { get; init; }
    public required ItemSubclass SoundOverrideSubclass { get; init; }
    public required uint DisplayID { get; init; }
    public required uint Quality { get; init; }
    public required uint Flags { get; init; }
    public required uint Flags2 { get; init; }
    public required uint BuyCount { get; init; }
    public required uint BuyPrice { get; init; } // is this int?
    public required uint SellPrice { get; init; }
    public required InventoryType InventoryType { get; init; }
    public required Class AllowableClass { get; init; }
    public required Race AllowableRace { get; init; }
    public required uint ItemLevel { get; init; }
    public required uint RequiredLevel { get; init; }
    public required uint RequiredSkill { get; init; }
    public required uint RequiredSkillRank { get; init; }
    public required uint RequiredSpell { get; init; }
    public required uint RequiredHonorRank { get; init; }
    public required uint RequiredCityRank { get; init; }
    public required uint RequiredReputationFaction { get; init; }
    public required uint RequiredReputationRank { get; init; }
    public required uint MaxCount { get; init; } //int? <= 0 no limit
    public required int Stackable { get; init; } // 0 not stackable, -1 player coin info tab
    public required uint ContainerSlots { get; init; }
    public required uint StatsCount { get; init; }
    public required ItemStat[] ItemStats { get; init; }
    public required uint ScalingStatDistribution { get; init; }
    public required uint ScalingStatValue { get; init; }
    public required Damage[] Damages { get; init; }
    public required uint[] Resistances { get; init; }
    public required uint Delay { get; init; }
    public required uint AmmoType { get; init; }
    public required float RangedAmmoRange { get; init; }
    public required ItemSpell[] Spells { get; init; }
    public required uint Bonding { get; init; }
    public required string Description { get; init; }
    public required uint PageText { get; init; }
    public required uint Language { get; init; }
    public required uint PageMaterial { get; init; }
    public required uint StartQuest { get; init; }
    public required uint LockID { get; init; }
    public required int Material { get; init; } // int?
    public required uint Sheath { get; init; }
    public required int RandomProperty { get; init; } //int
    public required int RandomSuffix { get; init; } //int
    public required uint Block { get; init; }
    public required uint ItemSet { get; init; }
    public required uint MaxDurability { get; init; }
    public required uint Area { get; init; }
    public required uint Map { get; init; }
    public required uint BagFamily { get; init; }
    public required uint TotemCategory { get; init; }
    public required ItemSocket[] Sockets { get; init; }
    public required uint SocketBonus { get; init; }
    public required uint GemProperties { get; init; }
    public required uint RequiredDisenchantSkill { get; init; }
    public required float ArmorDamageModifier { get; init; }
    public required uint Duration { get; init; }
    public required uint ItemLimitCategory { get; init; }
    public required uint HolidayID { get; init; }
    public required uint ScriptID { get; init; }
    public required uint DisenchantID { get; init; }
    public required uint FoodType { get; init; }
    public required uint MinMoneyLoot { get; init; }
    public required uint MaxMoneyLoot { get; init; }
    public required uint FlagsCu { get; init; }
}