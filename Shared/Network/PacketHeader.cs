namespace Shared.Network;

public ref struct ServerPacketHeader
{
    ushort _length;
    public ushort Length
    {
        get => (ushort)((_length >> 8) + (_length << 8));
        set => _length = (ushort)((value >> 8) + (value << 8));
    }
    public ushort LengthBigEndian
    {
        get => _length;
    }
    public Opcode Opcode { get; init; }

    public ServerPacketHeader(ushort length, Opcode opcode)
    {
        Opcode = opcode;
        length += 2;
        _length = (ushort)((length >> 8) + (length << 8));
    }
}

public ref struct ClientPacketHeader
{
    uint _length;
    public uint LengthBigEndian
    {
        get => (ushort)((_length >> 8) + (_length << 8));
        set => _length = (ushort)((_length >> 8) + (_length << 8));
    }
    public uint Length
    {
        get => _length;
    }
    public Opcode Opcode { get; init; }

    public ClientPacketHeader(uint length, Opcode opcode)
    {
        Opcode = opcode;
        _length = (ushort)((length >> 8) + (length << 8));
        _length -= 4;
    }
}
