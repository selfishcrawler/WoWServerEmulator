using System.Security.Cryptography;
using Game.Network;

namespace Game.Cryptography;

public class ARC4
{
    private static readonly byte[] encryptionKey = new byte[] { 0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA, 0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57 };
    private static readonly byte[] decryptionKey = new byte[] { 0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5, 0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE };

    private readonly byte[] _encryptionState;
    private readonly byte[] _decryptionState;
    private byte encrX, encrY, decrX, decrY;

    public ARC4(ReadOnlySpan<byte> sessionKey)
    {
        _encryptionState = new byte[256];
        _decryptionState = new byte[256];
        encrX = encrY = decrX = decrY = 0;
        Span<byte> hash = stackalloc byte[20];
        HMACSHA1.HashData(encryptionKey, sessionKey, hash);
        Init(hash, _encryptionState);

        HMACSHA1.HashData(decryptionKey, sessionKey, hash);
        Init(hash, _decryptionState);

        Span<byte> drop = stackalloc byte[1024];
        Encrypt(drop);
        Decrypt(drop);
    }

    private static void Init(ReadOnlySpan<byte> key, Span<byte> state)
    {
        byte index1 = 0, index2 = 0;
        for (int i = 0; i <= byte.MaxValue; i++)
            state[i] = (byte)i;

        for (int i = 0; i < 256; i++)
        {
            index2 = unchecked(((byte)(key[index1] + state[i] + index2)));
            (state[i], state[index2]) = (state[index2], state[i]);
            index1 = (byte)((index1 + 1) % key.Length);
        }
    }

    private void SwapBytes(Span<byte> state, int x, int y)
    {
        (state[x], state[y]) = (state[y], state[x]);
    }

    private void Encrypt(Span<byte> data)
    {
        unchecked
        {
            for (int i = 0; i < data.Length; i++)
            {
                encrX = (byte)(encrX + 1);
                encrY = (byte)(encrY + _encryptionState[encrX]);

                SwapBytes(_encryptionState, encrX, encrY);

                byte xorIndex = (byte)(_encryptionState[encrX] + _encryptionState[encrY]);
                data[i] ^= _encryptionState[xorIndex];
            }
        }
    }

    public void Encrypt(ref ServerPacketHeader header)
    {
        Span<byte> data = stackalloc byte[4];
        BitConverter.GetBytes(header.LengthBigEndian).CopyTo(data);
        BitConverter.GetBytes((ushort)header.Opcode).CopyTo(data[2..]);
        Encrypt(data);
        header.LengthBigEndian = BitConverter.ToUInt16(data);
        header.Opcode = (Opcode)BitConverter.ToUInt16(data[2..]);
    }

    public ClientPacketHeader Decrypt(Span<byte> data)
    {
        unchecked
        {
            for (int i = 0; i < data.Length; i++)
            {
                decrX = (byte)(decrX + 1);
                decrY = (byte)(decrY + _decryptionState[decrX]);

                SwapBytes(_decryptionState, decrX, decrY);

                byte xorIndex = (byte)(_decryptionState[decrX] + _decryptionState[decrY]);
                data[i] ^= _decryptionState[xorIndex];
            }
        }
        var header = new ClientPacketHeader(BitConverter.ToUInt16(data), (Opcode)BitConverter.ToUInt32(data[2..]));
        return header;
    }
}