namespace Game.Entities;

using Game.Prototypes;
using static EUnitFields;

public class Creature : Unit
{
    public override uint HighGuid => (uint)Entities.HighGuid.Unit;
    protected override ObjectType ObjectType => ObjectType.Unit;
    protected override TypeMask TypeMask => TypeMask.Unit | TypeMask.Object;

    public new required CreaturePrototype Prototype { get => (CreaturePrototype)base.Prototype; init => base.Prototype = value; }
    public override required uint Faction { get => base.Faction; init => base.Faction = value; }

    public Creature() : base((int)UNIT_END)
    {
        SetField(UNIT_NPC_FLAGS, 0);
        SetField(UNIT_DYNAMIC_FLAGS, 0);
        SetField(UNIT_FIELD_FLAGS, 0x8);
        SetField(UNIT_FIELD_COMBATREACH, 10f);
        SetField(UNIT_FIELD_BYTES_1, 0);
        SetField(UNIT_FIELD_BOUNDINGRADIUS, 30f);
    }
    public void BuildCreatePacket(MemoryStream ms)
    {
        BuildPacket(ObjectUpdateType.CreateObject, ms);
        ObjectUpdateFlag updateFlags = ObjectUpdateFlag.None;
        if (Alive)
            updateFlags |= ObjectUpdateFlag.Living;
        ms.Write(updateFlags);

        BuildMovementBlock(ms);
        WriteFullTable(ms);
    }
}