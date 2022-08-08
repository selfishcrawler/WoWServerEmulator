using System.Text;
using Shared.RealmInfo;

namespace AuthServer.Realms;

public class Realm
{
    private readonly int _addressOffset;
    private readonly int _populationOffset;
    private readonly byte[] _bytes;
    private float _population;
    public RealmType RealmType
    {
        get => (RealmType)_bytes[0];
        init => _bytes[0] = (byte)value;
    }
    public bool Locked
    {
        get => _bytes[1] != 0;
        set => _bytes[1] = (byte)(value ? 1 : 0);
    }
    public RealmFlags Flags
    {
        get => (RealmFlags)_bytes[2];
        init => _bytes[2] = (byte)value;
    }
    public string Name { get; init; }
    public string Address { get; init; }
    public float Population
    {
        get => _population;
        set
        {
            _population = value;
            BitConverter.TryWriteBytes(_bytes[_populationOffset..], _population);
        }
    }
    
    //characters count here

    public RealmTimeZone TimeZone
    {
        get => (RealmTimeZone)_bytes[_populationOffset + 4];
        init => _bytes[_populationOffset + 4] = (byte)value;
    }
    public byte ID
    {
        get => _bytes[_populationOffset + 5];
        init => _bytes[_populationOffset + 5] = value;
    }
    //SpecifyBuild
    public byte[] Version
    {
        get => _bytes.AsSpan()[(_populationOffset + 6)..(_populationOffset + 8)].ToArray();
        init
        {
            if (value.Length != 3)
                throw new ArgumentException("Wrong version lenght");
            value.CopyTo(_bytes.AsSpan()[(_populationOffset + 6)..]);
        }
    }
    public ushort Build
    {
        get => BitConverter.ToUInt16(_bytes.AsSpan()[(_populationOffset + 9)..]);
        init => BitConverter.TryWriteBytes(_bytes[(_populationOffset + 9)..], value);
    }

    public ushort Length
    {
        get => (ushort)(Flags.HasFlag(RealmFlags.SPECIFY_BUILD) ? _bytes.Length + 1 : _bytes.Length - 4);
    }

    public Realm(byte ID, string name, string address)
    {
        Name = name;
        Address = address;
        var nameBytes = Encoding.UTF8.GetBytes(Name + '\0');
        var addressBytes = Encoding.UTF8.GetBytes(Address + '\0');
        _bytes = new byte[14 + nameBytes.Length + addressBytes.Length];
        _addressOffset = 3 + nameBytes.Length;
        _populationOffset = _addressOffset + addressBytes.Length;
        nameBytes.CopyTo(_bytes.AsSpan()[3..]);
        addressBytes.CopyTo(_bytes.AsSpan()[(_addressOffset)..]);
        this.ID = ID;
    }

    public ReadOnlySpan<byte> GetPrefixBytes()
    {
        return _bytes.AsSpan()[2..(_populationOffset + 4)];
    }

    public ReadOnlySpan<byte> GetSuffixBytes()
    {
        if (Flags.HasFlag(RealmFlags.SPECIFY_BUILD))
            return _bytes.AsSpan()[(_populationOffset + 4)..];
        return _bytes.AsSpan()[(_populationOffset + 4)..(_populationOffset+6)];
    }
}