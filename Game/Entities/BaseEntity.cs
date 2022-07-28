using System.Collections;

namespace Game.Entities;
using static EObjectFields;

public struct Position
{
    public float X, Y, Z, Orientation;
}

public abstract class BaseEntity
{
    protected readonly BitArray _mask;
    protected readonly SortedDictionary<int, uint> _updateTable;
    protected readonly uint _guid, _entry;
    protected readonly byte[] _packedGuid;
    protected readonly byte _maskSize;
    protected float _scale;
    protected abstract ObjectType ObjectType { get; }
    protected abstract TypeMask TypeMask { get; }

    public uint Guid
    {
        get => _guid;
        init
        {
            _guid = value;
            Span<byte> packed = stackalloc byte[sizeof(ulong) + 1];
            packed[0] = 0; //guid mask
            var guidBytes = BitConverter.GetBytes(_guid);
            int count = 1;
            for (int i = 0; i < sizeof(uint); i++)
                if (guidBytes[i] != 0)
                {
                    packed[0] |= (byte)(1 << i);
                    packed[count++] = guidBytes[i];
                }
            for (int i = 0; i < sizeof(uint); i++)
            {
                if (HighGuid[i] != 0)
                {
                    packed[0] |= (byte)(1 << (i+4));
                    packed[count++] = HighGuid[i];
                }
            }
            _packedGuid = packed[..count].ToArray();
        }
    }

    public ReadOnlySpan<byte> PackedGuid => _packedGuid.AsSpan();
    public ReadOnlySpan<byte> LowGuid => BitConverter.GetBytes(_guid);
    public abstract ReadOnlySpan<byte> HighGuid { get; }
    public virtual uint Entry
    {
        get => _entry;
        init
        {
            _entry = value;
            SetField(OBJECT_FIELD_ENTRY, value);
        }
    }
    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            SetField(OBJECT_FIELD_SCALE_X, value);
        }
    }
    public Position Position { get; set; }
    public string Name { get; init; }

    protected BaseEntity(uint guid, int bitCount)
    {
        Guid = guid;
        _mask = new BitArray(bitCount, false);
        _updateTable = new SortedDictionary<int, uint>();
        _maskSize = (byte)((bitCount + 31) / 32);

        SetField(OBJECT_FIELD_GUID, BitConverter.ToUInt32(LowGuid));
        SetField(OBJECT_FIELD_GUID + 1, BitConverter.ToUInt32(HighGuid));
        SetField(OBJECT_FIELD_TYPE, TypeMask);
        Scale = 1.0f;
    }

    public void SetPosition(float x, float y, float z, float o)
    {
        Position = new Position()
        {
            X = x,
            Y = y,
            Z = z,
            Orientation = o,
        };
    }

    protected void SetField<TIndex>(TIndex index, uint value) where TIndex : Enum
    {
        int _index = Convert.ToInt32(index);
        _mask.Set(_index, true);
        _updateTable[_index] = value;
    }

    protected void SetField<TIndex>(TIndex index, float value) where TIndex : Enum
    {
        int _index = Convert.ToInt32(index);
        _mask.Set(_index, true);
        _updateTable[_index] = BitConverter.SingleToUInt32Bits(value);
    }
    protected void SetField<TIndex, TValue>(TIndex index, TValue value) where TIndex : Enum where TValue : Enum
    {
        int _index = Convert.ToInt32(index);
        _mask.Set(_index, true);
        _updateTable[_index] = Convert.ToUInt32(value);
    }

    protected void WriteUpdateTable(MemoryStream ms)
    {
        ms.Write(_maskSize);

        byte[] maskBuffer = new byte[_maskSize * sizeof(uint)];
        _mask.CopyTo(maskBuffer, 0);
        ms.Write(maskBuffer);

        foreach (uint val in _updateTable.Values)
            ms.Write(val);

        //_mask.SetAll(false);
        //_updateTable.Clear();
    }

    protected virtual void BuildUpdatePacket(ObjectUpdateType updateType, MemoryStream ms)
    {
        ms.Write(updateType);
        ms.Write(_packedGuid);
        ms.Write(ObjectType);
    }
}