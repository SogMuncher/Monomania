using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


// Stores client ID and name for a player. Can be expanded to store other fields later.
// We should preserve the session manager between scenes for storing critical data.(Can be renamed) 
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong clientID;
    public FixedString32Bytes name;
    
    public PlayerData(ulong clientID, FixedString32Bytes name)
    {
        this.clientID = clientID;
        this.name = name;
    }
    
    public bool Equals(PlayerData other)
    {
        return other.clientID == clientID;

    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref name);
        
    }
}

//Stores session related information. For now just stores a list of active players in the scene.
public class SessionManager : NetworkBehaviour
{
    public static SessionManager instance { get; private set; }
    public NetworkList<PlayerData> players { get; private set; }

    private void Awake()
    {
        instance = this;
        players = new NetworkList<PlayerData>();
    }
    //Add player to the network list when they connect
    public void AddPlayer(ulong clientID)
    {
        if (!IsServer) return;
        string playerName = clientID == NetworkManager.ServerClientId ? "Host" : $"Player:{clientID}";
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientID == clientID)
            {
                return;
            }
        }
        players.Add(new PlayerData(clientID, playerName));
        Debug.Log($"[Lobby] Added Player: {clientID}");
    }
    //remove player from the network list when they disconnect
    public void RemovePlayer(ulong clientID)
    {
        if (!IsServer) return;
        for (int i = players.Count - 1; i >= 0; i--)
        {
            if (players[i].clientID == clientID)
            {
                players.RemoveAt(i);
                Debug.Log($"[Lobby] Removed Player: {clientID}");
                return;
            }
        }
    }
    // When the network inits we want to start tracking network changes for maintaining the player list. only the host client should have access.
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;

            // Add host (server) immediately
            AddPlayer(NetworkManager.ServerClientId);
        }
        players.OnListChanged += change => Debug.Log($"Players changed: {players.Count}");
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        if (instance == this) instance = null;
    }

    private void OnClientConnected(ulong clientId)
    {
        AddPlayer(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        RemovePlayer(clientId);
    }
}
