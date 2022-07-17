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
        set => _length = value;
    }
    public Opcode Opcode { get; set; }

    public ServerPacketHeader(ushort length, Opcode opcode)
    {
        Opcode = opcode;
        length += 2;
        _length = (ushort)((length >> 8) + (length << 8));
    }
}

public ref struct ClientPacketHeader
{
    ushort _length;
    public ushort Length
    {
        get => _length;
    }
    public Opcode Opcode { get; set; }

    public ClientPacketHeader(ushort length, Opcode opcode)
    {
        Opcode = opcode;
        _length = (ushort)((length >> 8) + (length << 8));
        _length -= 4;
    }
}
