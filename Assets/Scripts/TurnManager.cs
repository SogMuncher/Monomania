using Sirenix.OdinInspector;
using System;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    public event Action<int> OnTurnChanged; // Event that clients can subscribe to for turn changes, passes the new active player index>

    //                       //
    // ===== Variables ===== //
    //                       //

    [SerializeField, TabGroup("Tunables")]
    public bool IsRandomStartingPlayer = false;

    // Offset added to turn index when calculating active player index. This is set at the start of all games where the starting player is not player 0.
    [ReadOnly, ShowInInspector]
    public int TurnIndexOffset_Debug => TurnIndexOffset.Value;
    public NetworkVariable<int> TurnIndexOffset = new NetworkVariable<int>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);


    // Current turn index indicates the total number of turns that have passed since the start of the game, used to track turn order as well.
    [ReadOnly, ShowInInspector]
    public int CurrentTurnIndex_Debug => CurrentTurnIndex.Value;
    public NetworkVariable<int> CurrentTurnIndex = new NetworkVariable<int>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);

    // Total number of players in the session, determined at the start of the game by checking SessionManager
    [ReadOnly, ShowInInspector]
    public int CurrentPlayerCount_Debug => CurrentPlayerCount.Value;
    public NetworkVariable<int> CurrentPlayerCount = new NetworkVariable<int>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);

    // Active player index indicates which player's turn it currently is (0-based index, corresponds to player list in SessionManager)
    [ReadOnly, ShowInInspector]
    public int CurrentActivePlayerIndex_Debug => CurrentActivePlayerIndex.Value;
    public NetworkVariable<int> CurrentActivePlayerIndex = new NetworkVariable<int>(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);


    //                    //
    //===== Methods ===== //
    //                    //

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
 
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentTurnIndex.Value = 0;
            CurrentPlayerCount.Value = SessionManager.Instance.Players.Count;

            if (IsRandomStartingPlayer)
            {
                TurnIndexOffset.Value = UnityEngine.Random.Range(0, CurrentPlayerCount.Value);
                CurrentActivePlayerIndex.Value = TurnIndexOffset.Value;
            }
            else
            {
                TurnIndexOffset.Value = 0;
                CurrentActivePlayerIndex.Value = 0; // Start with player 1
            }

            // Subscribe to player list changes to update player count and adjust turn order if necessary
            SessionManager.Instance.Players.OnListChanged += OnPlayerListChanged;
        }

        // Listen for turn changes on all clients
        CurrentActivePlayerIndex.OnValueChanged += OnActivePlayerIndexChanged;
    }

    private void OnPlayerListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        //TODO: Handle player count changes mid-game (e.g. if a player leaves or joins, we need to update CurrentPlayerCount and adjust turn order if necessary)
    }

    private void OnActivePlayerIndexChanged(int previousValue, int newValue)
    {
        OnTurnChanged?.Invoke(newValue);
    }

    //                         //
    // ===== RPC Methods ===== //
    //                         //

    // Called by clients to end their turn
    [Rpc(SendTo.Server)]
    public void Server_EndTurn_Rpc(RpcParams rpcParams = default)
    {
        Debug.Log("TURNMANAGER: Received request to end turn from client " + rpcParams.Receive.SenderClientId);

        // Validate that the sender is the current active player
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        ulong activePlayerClientId = SessionManager.Instance.Players[CurrentActivePlayerIndex.Value].clientID;

        bool senderIsCurrentPlayer = activePlayerClientId == senderClientId;

        if (!senderIsCurrentPlayer)
        {
            Debug.LogWarning($"TURNMANAGER: Client {senderClientId} attempted to end turn, but it's not their turn. It is Client {activePlayerClientId}'s turn.");
            return;
        }
        else if (senderIsCurrentPlayer)
        {
            Debug.Log($"TURNMANAGER: Client {senderClientId} ended their turn successfully.");
            // Increment the turn index and wrap around to the first player if necessary
            CurrentTurnIndex.Value++;
            CurrentActivePlayerIndex.Value = (CurrentTurnIndex.Value + TurnIndexOffset.Value) % CurrentPlayerCount.Value;

            Debug.Log($"TURNMANAGER: TurnIndex: {CurrentTurnIndex.Value}, ActivePlayerIndex: {CurrentActivePlayerIndex.Value}");
        }
    }
}
