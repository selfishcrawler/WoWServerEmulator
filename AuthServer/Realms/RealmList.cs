﻿using System.Net.Sockets;
using AuthServer.Enums;
using Shared.Database;
using Shared.RealmInfo;

namespace AuthServer.Realms;

public class RealmList
{
    private readonly Dictionary<int, Realm> _realms;

    public RealmList()
    {
        _realms = new Dictionary<int, Realm>();
        UpdateRealmList();
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

    public void UpdateRealmList()
    {
        foreach (var realmObject in Database.Login.ExecuteMultipleRaws(Database.Login.GetRealmList, null))
        {
            byte id = (byte)realmObject[0];
            string name = (string)realmObject[1];
            string address = $"{(string)realmObject[5]}:{(int)realmObject[6]}";
            var realm = new Realm(id, name, address)
            {
                RealmType = (RealmType)realmObject[2],
                Locked = (bool)realmObject[3],
                Flags = (RealmFlags)realmObject[4],
                Population = (float)realmObject[7],
                TimeZone = (RealmTimeZone)realmObject[8],
                Version = new byte[3], // maybe get version with build?
                Build = (ushort)(int)realmObject[9]
            };
            Add(realm);
        }
    }

    public void SendRealmListToNetworkStream(NetworkStream stream, Dictionary<int, (bool, byte)> accountValuesForRealm)
    {
        ushort length = 8;
        foreach (var realm in _realms.Values)
            length += realm.Length;
        Span<byte> buffer = stackalloc byte[9];
        buffer[0] = (byte)AuthCommand.REALMLIST;
        BitConverter.TryWriteBytes(buffer[1..], length);
        BitConverter.TryWriteBytes(buffer[3..], 0);
        BitConverter.TryWriteBytes(buffer[7..], (ushort)_realms.Count);

        stream.Write(buffer);
        foreach (var realm in _realms.Values)
        {
            (bool locked, byte characters) = accountValuesForRealm.GetValueOrDefault(realm.ID, (false, (byte)0));
            stream.WriteByte((byte)realm.RealmType);
            stream.WriteByte((byte)(realm.Locked && locked ? 1 : 0));
            stream.Write(realm.GetPrefixBytes());
            stream.WriteByte(characters);
            stream.Write(realm.GetSuffixBytes());
        }
    }
}
