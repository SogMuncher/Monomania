using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    //                       //
    // ===== Variables ===== //
    //                       //

    [SerializeField, TabGroup("Tunables")]
    public bool IsRandomStartingPlayer = false;

    // Offset added to turn index when calculating active player index. This is set at the start of all games where the starting player is not player 0.
    [ReadOnly, ShowInInspector]
    public int TurnIndexOffset_Debug => TurnIndexOffset.Value;
    public NetworkVariable<int> TurnIndexOffset          { get; private set; }


    // Current turn index indicates the total number of turns that have passed since the start of the game, used to track turn order as well.
    [ReadOnly, ShowInInspector]
    public int CurrentTurnIndex_Debug => CurrentTurnIndex.Value;
    public NetworkVariable<int> CurrentTurnIndex         { get; private set; }

    // Total number of players in the session, determined at the start of the game by checking SessionManager
    [ReadOnly, ShowInInspector]
    public int CurrentPlayerCount_Debug => CurrentPlayerCount.Value;
    public NetworkVariable<int> CurrentPlayerCount       { get; private set; }

    // Active player index indicates which player's turn it currently is (0-based index, corresponds to player list in SessionManager)
    [ReadOnly, ShowInInspector]
    public int CurrentActivePlayerIndex_Debug => CurrentActivePlayerIndex.Value;
    public NetworkVariable<int> CurrentActivePlayerIndex { get; private set; }

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
                TurnIndexOffset.Value = Random.Range(0, CurrentPlayerCount.Value);
                CurrentActivePlayerIndex.Value = TurnIndexOffset.Value;
            }
            else
            {
                TurnIndexOffset.Value = 0;
                CurrentActivePlayerIndex.Value = 0; // Start with player 1
            }
        }
    }

    //                         //
    // ===== RPC Methods ===== //
    //                         //

    // Called by clients to end their turn
    [Rpc(SendTo.Server)]
    public void Server_EndTurn_Rpc(RpcParams rpcParams = default)
    {
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
            // Increment the turn index and wrap around to the first player if necessary
            CurrentTurnIndex.Value++;
            CurrentActivePlayerIndex.Value = (CurrentTurnIndex.Value + TurnIndexOffset.Value) % CurrentPlayerCount.Value;
        }
    }
}
