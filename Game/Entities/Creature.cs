namespace Game.Entities;
using static EUnitFields;

public class Creature : Unit
{

    public override ReadOnlySpan<byte> HighGuid => throw new NotImplementedException();

    protected override ObjectType ObjectType => throw new NotImplementedException();

    protected override TypeMask TypeMask => TypeMask.Unit | TypeMask.Object;

    public Creature(uint guid) : base(guid, (int)UNIT_END)
    {
    }
}
