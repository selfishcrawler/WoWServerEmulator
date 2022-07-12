using System.Net.Sockets;
using AuthServer.Enums;

namespace AuthServer.Realms;

public class RealmList
{
    private readonly Dictionary<int, Realm> _realms;

    public RealmList()
    {
        _realms = new Dictionary<int, Realm>();
    }

    public Realm this[int index]
    {
        get => _realms[index];
    }

    public void Add(Realm realm)
    {
        if (_realms.ContainsKey(realm.ID))
            throw new ArgumentException("Realm already exists", realm.ID.ToString());
        _realms[realm.ID] = realm;
    }

    public void Remove(int ID)
    {
        _realms.Remove(ID);
    }

    public void SendRealmListToNetworkStream(NetworkStream stream, Dictionary<int, (bool, byte)> accountValuesForRealm)
    {
        ushort length = 8;
        foreach (var realm in _realms.Values)
            length += realm.Length;
        Span<byte> buffer = stackalloc byte[9];
        buffer[0] = (byte)AuthCommand.REALMLIST;
        BitConverter.GetBytes(length).CopyTo(buffer[1..]);
        BitConverter.GetBytes(0).CopyTo(buffer[3..]);
        BitConverter.GetBytes((ushort)_realms.Count).CopyTo(buffer[7..]);

        stream.Write(buffer);
        foreach (var realm in _realms.Values)
        {
            var accountValues = accountValuesForRealm.GetValueOrDefault(realm.ID, (false, (byte)0));
            stream.WriteByte((byte)realm.RealmType);
            stream.WriteByte((byte)(accountValues.Item1 ? 1 : 0));
            stream.Write(realm.GetPrefixBytes());
            stream.WriteByte(accountValues.Item2);
            stream.Write(realm.GetSuffixBytes());
        }
    }
}
