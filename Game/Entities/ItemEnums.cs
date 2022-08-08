namespace Game.Entities;

public enum InventoryType : uint
{
    NonEquip,
    Head,
    Neck,
    Shoulders,
    Body,
    Chest,
    Waist,
    Legs,
    Feet,
    Wrists,
    Hands,
    Finger,
    Trinket,
    Weapon,
    Shield,
    Ranged,
    Cloak,
    Weapon2H,
    Bag,
    Tabard,
    Robe,
    WeaponMainHand,
    WeaponOffHand,
    Holdable,
    Ammo,
    Thrown,
    RangedRight,
    Quiver,
    Relic
}

public enum EquipmentSlot : byte
{
    Head,
    Neck,
    Shoulders,
    Body,
    Chest,
    Waist,
    Legs,
    Feet,
    Wrists,
    Hands,
    Finger1,
    Finger2,
    Trinket1,
    Trinket2,
    Back,
    MainHand,
    OffHand,
    Ranged,
    Tabard,
    EQUIPMENT_SLOT_COUNT,
}

public enum ItemClass : uint
{
    Consumable,
    Container,
    Weapon,
    Gem,
    Armor,
    Reagent,
    Projectile,
    TradeGoods,
    Generic,
    Recipe,
    Money,
    Quiver,
    Quest,
    Key,
    Permanent,
    Misc,
    Glyph,
    MAX_ITEM_CLASS,
}

#pragma warning disable CA1069
public enum ItemSubclass : uint
{
    //Consumable
    Consumable = 0,
    Potion = 1,
    Elixir = 2,
    Flask = 3,
    Scroll = 4,
    Food = 5,
    ItemEnchantment = 6,
    Bandage = 7,
    ConsumableOther = 8,

    //Container
    Container = 0,
    SoulContainer = 1,
    HerbContainer = 2,
    EnchantingContainer = 3,
    EngineeringContainer = 4,
    GemContainer = 5,
    MiningContainer = 6,
    LeatherworkingContainer = 7,
    InscriptionContainer = 8,

    //Weapon
    Axe = 0,
    Axe2H = 1,
    Bow = 2,
    Gun = 3,
    Mace = 4,
    Mace2H = 5,
    Polearm = 6,
    Sword = 7,
    Sword2H = 8,
    _obsolete = 9,
    Staff = 10,
    Exotic = 11,
    Exotic2 = 12,
    Fist = 13,
    Misc = 14,
    Dagger = 15,
    Thrown = 16,
    Spear = 17,
    Crossbow = 18,
    Wand = 19,
    FishingPole = 20,

    //Gem
    GemRed = 0,
    GemBlue = 1,
    GemYellow = 2,
    GemPurple = 3,
    GemGreen = 4,
    GemOrange = 5,
    GemMeta = 6,
    GemSimple = 7,
    GemPrismatic = 8,

    //Armor
    ArmorMisc = 0,
    Cloth = 1,
    Leather = 2,
    Mail = 3,
    Plate = 4,
    Buckler = 5,
    Shield = 6,
    Libram = 7,
    Idol = 8,
    Totem = 9,
    Sigil = 10,

    //Reagent
    Reagent = 0,

    //Projectile
    ProjWand = 0,
    ProjBolt = 1,
    ProjArrow = 2,
    ProjBullet = 3,
    ProjThrown = 4,

    //TradeGoods
    TradeGoods = 0,
    Parts = 1,
    Explosives = 2,
    Devices = 3,
    Jewelcrafting = 4,
    GoodsCloth = 5,
    GoodsLeather = 6,
    MetalStone = 7,
    Meat = 8,
    Herb = 9,
    Elemental = 10,
    GoodsOther = 11,
    Enchanting = 12,
    Material = 13,
    ArmorEnchantment = 14,
    WeaponEnchantment = 15,

    //Generic
    Generic = 0,

    //Recipe
    Book = 0,
    LeatherworkingPattern = 1,
    TailoringPattern = 2,
    EngineeringSchematic = 3,
    Blacksmithing = 4,
    CookingRecipe = 5,
    AlchemyRecipe = 6,
    FirstAidManual = 7,
    EnchantingFormula = 8,
    FishingManual = 9,
    JewelcraftingRecipe = 10,

    //Money
    Money = 0,

    //Quiver
    Quiver0 = 0,
    Quiver1 = 1,
    Quiver = 2,
    AmmoPouch = 3,

    //Quest
    Quest = 0,

    //Key
    Key = 0,
    Lockpick = 1,

    //Permanent
    Permanent = 0,

    //Junk
    Junk = 0,
    JunkReagent = 1,
    JunkPet = 2,
    JunkHoliday = 3,
    JunkOther = 4,
    JunkMount = 5,

    //Glyph
    GlyphWarrior = Class.Warrior,
    GlyphPaladin = Class.Paladin,
    GlyphHunter = Class.Hunter,
    GlyphRogue = Class.Rogue,
    GlyphPriest = Class.Priest,
    GlyphDeathKnight = Class.DeathKnight,
    GlyphShaman = Class.Shaman,
    GlyphMage = Class.Mage,
    GlyphWarlock = Class.Warlock,
    GlyphDruid = Class.Druid,
}
#pragma warning restore CA1069