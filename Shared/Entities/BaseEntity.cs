using Shared.Extensions;
using System.Collections;

namespace Shared.Entities;

public abstract class BaseEntity
{
    protected readonly BitArray _mask;
    protected readonly SortedDictionary<int, uint> _updateTable;
    protected readonly uint _guid;
    protected readonly byte[] _packedGuid;
    protected readonly byte _maskSize;
    protected readonly uint _entry;

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
                    packed[0] |= (byte)(1 << i);
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
            SetField(EObjectFields.OBJECT_FIELD_ENTRY, value);
        }
    }

    protected BaseEntity(uint guid, int bitCount)
    {
        Guid = guid;
        _mask = new BitArray(bitCount, false);
        _updateTable = new SortedDictionary<int, uint>();
        _maskSize = (byte)((bitCount + 31) / 32);

        SetField(EObjectFields.OBJECT_FIELD_GUID, BitConverter.ToUInt32(LowGuid));
        SetField(EObjectFields.OBJECT_FIELD_GUID + 1, BitConverter.ToUInt32(HighGuid));
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

        byte[] maskBuffer = new byte[(_mask.Length + 8) / 8 + 2]; //?
        _mask.CopyTo(maskBuffer, 0);
        ms.Write(maskBuffer);

        foreach (uint val in _updateTable.Values)
            ms.Write(val);

        _mask.SetAll(false);
        _updateTable.Clear();
    }
}
