using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace AuthServer.Cryptography;

public class SRP6
{
    //constant section
    private static readonly BigInteger _N;
    private static readonly BigInteger _G;
    private static readonly BigInteger _K;
    private static readonly BigInteger _b;
    private static readonly byte[] NGHash;
    private const int L = 32; // size of all buffers
    private const int hashL = 20;
    private const byte K = 3;

    public static readonly byte[] N;
    public static readonly byte[] b;
    public const byte G = 7;

    //member section
    public byte[] Verifier { get; init; }
    public byte[] Salt { get; init; }
    public byte[] ServerPublicKey { get; init; }

    private readonly BigInteger _verifier;

    static SRP6()
    {
        _N = BigInteger.Parse("0894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7", NumberStyles.AllowHexSpecifier);
        N = _N.ToByteArray()[..L];
        _G = G;
        _K = K;

        b = RandomNumberGenerator.GetBytes(L);
        _b = new BigInteger(b, true);

        NGHash = SHA1.HashData(stackalloc byte[] { G });
        Span<byte> NHash = stackalloc byte[hashL];
        SHA1.HashData(N, NHash);
        for (int i = 0; i < hashL; i++)
            NGHash[i] ^= NHash[i];
    }

    public static ReadOnlySpan<byte> CalculateVerifier(string username, string password, byte[] salt)
    {
        Span<char> concat = stackalloc char[username.Length + password.Length + 1];
        username.CopyTo(concat);
        concat[username.Length] = ':';
        password.CopyTo(concat[(username.Length + 1)..]);
        for (int i = 0; i < concat.Length; i++)
            concat[i] = char.ToUpper(concat[i]);

        Span<byte> union = stackalloc byte[L + hashL];
        salt.CopyTo(union);
        SHA1.HashData(Encoding.UTF8.GetBytes(concat.ToArray()), union[L..]);

        Span<byte> x = stackalloc byte[hashL];
        SHA1.HashData(union, x);
        var xAsBigInteger = new BigInteger(x, true);
        var result = BigInteger.ModPow(_G, xAsBigInteger, _N);

        return result.ToByteArray().AsSpan()[..L];
    }

    public static ReadOnlySpan<byte> GenerateSalt() => RandomNumberGenerator.GetBytes(L);

    public SRP6(byte[] Verifier, byte[] Salt)
    {
        ArgumentNullException.ThrowIfNull(Verifier, nameof(Verifier));
        if (Verifier.Length != L)
            throw new ArgumentException("Wrong verifier length", nameof(Verifier));

        this.Verifier = Verifier;
        this.Salt = Salt;

        _verifier = new BigInteger(Verifier, true);
        var interim = _K * _verifier + BigInteger.ModPow(_G, _b, _N);
        var spk = (interim % _N).ToByteArray();
        ServerPublicKey = spk.Length == L ? spk : spk[..L];
    }

    public ReadOnlySpan<byte> GetM2(ReadOnlySpan<byte> A, ReadOnlySpan<byte> M1, string username)
    {
        Span<byte> concat = stackalloc byte[2 * L];
        A.CopyTo(concat);
        ServerPublicKey.CopyTo(concat[L..]);

        Span<byte> hash0 = stackalloc byte[hashL];
        SHA1.HashData(concat, hash0);
        var u = new BigInteger(hash0, true);
        var a = new BigInteger(A, true);

        var interim = (a * BigInteger.ModPow(_verifier, u, _N));
        Span<byte> S = stackalloc byte[L];
        BigInteger.ModPow(interim, _b, _N).TryWriteBytes(S, out _, true);

        //interleave
        while (S[0] == 0)
            S = S[2..];

        Span<byte> buf0 = stackalloc byte[S.Length / 2];
        Span<byte> buf1 = stackalloc byte[S.Length / 2];
        for (int i = 0; i < S.Length / 2; i++)
        {
            buf0[i] = S[2 * i];
            buf1[i] = S[2 * i + 1];
        }

        Span<byte> hash1 = stackalloc byte[hashL];
        SHA1.HashData(buf0, hash0);
        SHA1.HashData(buf1, hash1);

        Span<byte> m2 = stackalloc byte[hashL * 2 + hashL + L];
        for (int i = 0; i < hashL; i++)
        {
            m2[2 * i + 52] = hash0[i];
            m2[2 * i + 53] = hash1[i];
        }
        //interleave end

        //calculated client proof
        //SHA1(NGHash | userhash | salt | A | B | sessionKey)
        Span<byte> bytes = stackalloc byte[hashL + hashL + L + L + L + hashL * 2];
        NGHash.CopyTo(bytes);
        SHA1.HashData(Encoding.UTF8.GetBytes(username), bytes[hashL..]);
        Salt.CopyTo(bytes[(hashL * 2)..]);
        A.CopyTo(bytes[(hashL * 2 + L)..]);
        ServerPublicKey.CopyTo(bytes[(hashL * 2 + 2 * L)..]);
        m2[52..].CopyTo(bytes[(hashL * 2 + 3 * L)..]);
        Span<byte> m1 = stackalloc byte[hashL];
        SHA1.HashData(bytes, m1);
        if (!m1.SequenceEqual(M1))
            return ReadOnlySpan<byte>.Empty;

        A.CopyTo(m2);
        M1.CopyTo(m2[L..]);
        return SHA1.HashData(m2);
    }
}