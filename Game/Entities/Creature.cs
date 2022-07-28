namespace Game.Entities;
using static EUnitFields;

public class Creature : Unit
{
    private static readonly byte[] _highGuidBytes = BitConverter.GetBytes((uint)Entities.HighGuid.Unit);
    public override ReadOnlySpan<byte> HighGuid => _highGuidBytes;
    protected override ObjectType ObjectType => ObjectType.Unit;
    protected override TypeMask TypeMask => TypeMask.Unit | TypeMask.Object;

    public Creature(uint guid) : base(guid, (int)UNIT_END)
    {
        SetField(UNIT_FIELD_FACTIONTEMPLATE, 954);
        SetField(UNIT_NPC_FLAGS, 0);
        SetField(UNIT_DYNAMIC_FLAGS, 0);
        SetField(UNIT_FIELD_FLAGS, 0x8);
        SetField(UNIT_FIELD_COMBATREACH, 10f);
        SetField(UNIT_FIELD_BYTES_1, 0);
        SetField(UNIT_FIELD_BOUNDINGRADIUS, 30f);
    }
    public void BuildUpdatePacket(MemoryStream ms)
    {
        BuildUpdatePacket(ObjectUpdateType.CreateObject, ms);
        ObjectUpdateFlag updateFlags = ObjectUpdateFlag.None;
        if (Alive)
            updateFlags |= ObjectUpdateFlag.Living;
        ms.Write(updateFlags);

        BuildMovementBlock(ms);
        WriteUpdateTable(ms);
    }
}