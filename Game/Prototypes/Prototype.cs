using System.Text;

namespace Game.Prototypes;

public abstract class Prototype
{
    public required uint Entry { get; init; }
    public required string Name { get; init; }


    private byte[] _cachedBuffer;
    private byte[] BuildResponceBuffer()
    {
        using MemoryStream ms = new(1000);

        WritePrototypeValues(ms);

        byte[] retbuf = new byte[ms.Position];
        ms.Position = 0;
        ms.ReadExactly(retbuf, 0, retbuf.Length);
        return retbuf;
    }
    protected abstract void WritePrototypeValues(MemoryStream ms);

    public ReadOnlySpan<byte> PrototypeResponseBytes => _cachedBuffer ??= BuildResponceBuffer();
}