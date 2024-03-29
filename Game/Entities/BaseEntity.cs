﻿using System.Collections;
using Game.Prototypes;

namespace Game.Entities;
using static EObjectFields;

public struct Position
{
    public float X, Y, Z, Orientation;

    public float GetDistance2D(in Position other) => MathF.Sqrt(GetSquaredDistance2D(other));

    public float GetDistance3D(in Position other) => MathF.Sqrt(GetSquaredDistance3D(other));

    public float GetSquaredDistance2D(in Position other)
    {
        var xdiff = X - other.X;
        var ydiff = Y - other.Y;
        return xdiff * xdiff + ydiff * ydiff;
    }

    public float GetSquaredDistance3D(in Position other)
    {
        var zdiff = Z - other.Z;
        return GetSquaredDistance2D(other) + zdiff * zdiff;
    }
}

public abstract class BaseEntity
{
    protected readonly BitArray _fullMask, _updateMask;
    protected readonly SortedDictionary<int, uint> _fullTable, _updateTable;
    protected readonly uint _guid;
    protected readonly byte[] _packedGuid;
    protected readonly byte _maskSize;
    protected readonly Prototype _prototype;
    protected float _scale;
    protected abstract ObjectType ObjectType { get; }
    protected abstract TypeMask TypeMask { get; }

    public ReadOnlySpan<byte> PackedGuid => _packedGuid;
    public abstract uint HighGuid { get; }

    public required unsafe uint Guid
    {
        get => _guid;
        init
        {
            _guid = value;
            Span<byte> packed = stackalloc byte[sizeof(ulong) + 1];
            packed[0] = 0; //guid mask
            var guidBytes = new ReadOnlySpan<byte>(&value, sizeof(uint));
            int count = 1;
            for (int i = 0; i < sizeof(uint); i++)
            {
                if (guidBytes[i] != 0)
                {
                    packed[0] |= (byte)(1 << i);
                    packed[count++] = guidBytes[i];
                }
            }
            uint highguid = HighGuid;
            var highGuidBytes = new ReadOnlySpan<byte>(&highguid, sizeof(uint));
            for (int i = 0; i < sizeof(uint); i++)
            {
                if (highGuidBytes[i] != 0)
                {
                    packed[0] |= (byte)(1 << (i + 4));
                    packed[count++] = highGuidBytes[i];
                }
            }
            _packedGuid = packed[..count].ToArray();
            SetField(OBJECT_FIELD_GUID, _guid);
            SetField(OBJECT_FIELD_GUID + 1, highguid);
        }
    }

    public virtual Prototype Prototype
    {
        get => _prototype;
        init
        {
            _prototype = value;
            SetField(OBJECT_FIELD_ENTRY, _prototype.Entry);
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

    public virtual string Name { get; init; }

    protected BaseEntity(int bitCount)
    {
        _fullMask = new BitArray(bitCount, false);
        _updateMask = new BitArray(bitCount, false);
        _fullTable = new SortedDictionary<int, uint>();
        _updateTable = new SortedDictionary<int, uint>();
        _maskSize = (byte)((bitCount + 31) / 32);

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
        _updateMask.Set(_index, true);
        _fullMask.Set(_index, true);
        _updateTable[_index] = value;
        _fullTable[_index] = value;
    }

    protected void SetField<TIndex>(TIndex index, float value) where TIndex : Enum
    {
        int _index = Convert.ToInt32(index);
        _updateMask.Set(_index, true);
        _fullMask.Set(_index, true);
        _updateTable[_index] = BitConverter.SingleToUInt32Bits(value);
        _fullTable[_index] = BitConverter.SingleToUInt32Bits(value);
    }
    protected void SetField<TIndex, TValue>(TIndex index, TValue value) where TIndex : Enum where TValue : Enum
    {
        int _index = Convert.ToInt32(index);
        _updateMask.Set(_index, true);
        _fullMask.Set(_index, true);
        _updateTable[_index] = Convert.ToUInt32(value);
        _fullTable[_index] = Convert.ToUInt32(value);
    }

    protected void WriteFullTable(MemoryStream ms)
    {
        ms.Write(_maskSize);
        byte[] maskBuffer = new byte[_maskSize * sizeof(uint)];
        _fullMask.CopyTo(maskBuffer, 0);
        ms.Write(maskBuffer);

        foreach (uint val in _fullTable.Values)
            ms.Write(val);
    }

    protected void WriteUpdateTable(MemoryStream ms)
    {
        ms.Write(_maskSize);

        byte[] maskBuffer = new byte[_maskSize * sizeof(uint)];
        _updateMask.CopyTo(maskBuffer, 0);
        ms.Write(maskBuffer);

        foreach (uint val in _updateTable.Values)
            ms.Write(val);

        _updateMask.SetAll(false);
        _updateTable.Clear();
    }

    protected virtual void BuildPacket(ObjectUpdateType updateType, MemoryStream ms)
    {
        ms.Write(updateType);
        ms.Write(_packedGuid);
        ms.Write(ObjectType);
    }
}