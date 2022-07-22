using Shared.Extensions;

namespace Shared.Entities;
using static EObjectFields;
using static EUnitFields;

public sealed class Player : Unit
{
    private static readonly byte[] _highGuidBytes = BitConverter.GetBytes((uint)Entities.HighGuid.Player);
    public override ReadOnlySpan<byte> HighGuid => _highGuidBytes;

    public Player(uint guid) : base(guid, (int)PLAYER_END)
    {

        SetField(OBJECT_FIELD_TYPE, TypeMask.Player | TypeMask.Unit | TypeMask.Object);
        SetField(OBJECT_FIELD_SCALE_X, BitConverter.ToUInt32(stackalloc byte[] { 0x00, 0x00, 0x80, 0x3f }));
        SetField(UNIT_FIELD_BYTES_0, BitConverter.ToUInt32(stackalloc byte[] { 0x01, 0x01, 0x01, 0x01 })); //race class gender powertype
        SetField(UNIT_FIELD_LEVEL, 80);
        SetField(UNIT_FIELD_FACTIONTEMPLATE, 1);
        SetField(UNIT_FIELD_DISPLAYID, 19724);
        SetField(UNIT_FIELD_NATIVEDISPLAYID, 19724);

        _x = -8949.95F;
        _y = -132.493F;
        _z = 83.5312F;
        _o = 0f;

    }

    public void BuildUpdatePacket(MemoryStream ms, bool self = true)
    {
        ms.Write((uint)1); //block count
        ms.Write(ObjectUpdateType.CreateObject);
        ms.Write(_packedGuid);
        ms.Write(ObjectType.Player);
        ObjectUpdateFlag updateFlags = ObjectUpdateFlag.Living;
        if (self)
            updateFlags |= ObjectUpdateFlag.Self;
        ms.Write(updateFlags);
        ms.Write((uint)0); //movement flags
        ms.Write((ushort)0); //extra movement flags
        ms.Write((uint)DateTimeOffset.Now.ToUnixTimeSeconds()); //timestamp
        ms.Write(_x);
        ms.Write(_y);
        ms.Write(_z);
        ms.Write(_o);
        ms.Write((uint)1); //fall time

        ms.Write(1f);           //walk speed
        ms.Write(70f);          //run speed
        ms.Write(4.5f);         //run back speed
        ms.Write(0f);           //swim speed
        ms.Write(0f);           //swim back speed
        ms.Write(0f);           //fly speed
        ms.Write(0f);           //back fly speed
        ms.Write(3.1415405f);     //turn rate
        ms.Write(0f);           //pitch rate

        WriteUpdateTable(ms);
    }
}
