namespace Game.Entities;

public enum Race : byte
{
    Human = 1,
    Orc,
    Dwarf,
    NightElf,
    Undead,
    Tauren,
    Gnome,
    Troll,
    Goblin,
    BloodElf,
    Draenei
}

public enum Class : byte
{
    Warrior = 1,
    Paladin,
    Hunter,
    Rogue,
    Priest,
    DeathKnight,
    Shaman,
    Mage,
    Warlock,
    Monk,
    Druid,
}

public enum Gender : byte
{
    Male,
    Female,
}

public enum PowerType : byte
{
    Mana,
    Rage,
    Focus,
    Energy,
    Happiness,
    Runes,
    RunicPower
}