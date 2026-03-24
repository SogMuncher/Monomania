using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class MainMenuUI : MonoBehaviour
{
    public event Action<bool> OnMenuVisibilityChanged;

    [Header("Menus")]
    [SerializeField] private GameObject corePanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject lobbyPanel;

    // References for the network relay and the session manager for storing player data
    [Header("Refs")]
    [SerializeField] private RelayConnector relay;
    [SerializeField] private SessionManager sessionManager;

    // UI elements for the first menu (joining/hosting init menu)
    [Header("Main Menu UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button InitLobbyButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button returnButton;

    [SerializeField] private GameObject joinSection;
    [SerializeField] private GameObject HostSection;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_InputField PlayerNameInput;
    [SerializeField] private Button connectButton;
   

    [SerializeField] private TMP_Text joinCodeLabel;
    [SerializeField] private TMP_Text statusLabel;

    // UI elements for the player lobby before game init
    [Header("Lobby UI")]
    [SerializeField] private Transform listContainer; 
    [SerializeField] private PlayerRowUI rowPrefab;
    [SerializeField] private Button startGameButton;

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = "";

    private readonly List<GameObject> spawnedRows = new();

    private void Reset()
    {
        relay = FindFirstObjectByType<RelayConnector>();
        sessionManager = FindFirstObjectByType<SessionManager>();
    }

    private void Awake()
    {
        // Set menu to active, make sure lobby is hidden
        // these should definitely exist, error otherwise
        ShowMainMenu();

        // Button listeners
        hostButton.onClick.AddListener(OnClickHost);
        InitLobbyButton.onClick.AddListener(OnClickInitLobby);
        joinButton.onClick.AddListener(OnClickJoin);
        connectButton.onClick.AddListener(OnClickConnect);
        returnButton.onClick.AddListener(OnClickReturn);
        startGameButton.onClick.AddListener(OnClickStartGame);

        // Subscribe to the client connect/disconnect functions. Called when client connect/disconnects
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    //when the menu is destroyed, we want to stop listening for player connection changes
    private void OnDestroy()
    {
        UnbindSessionManager();
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    //funtion activates the main menu portion of the US
    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);

        ClearRows(); //clear the player list if it had data(previous connection)
        lobbyPanel.SetActive(false);
        returnButton.gameObject.SetActive(false);
        joinSection.SetActive(false);
        HostSection.SetActive(false);
        PlayerNameInput.gameObject.SetActive(false);
        joinCodeInput.text = "";
        joinCodeLabel.text = "Join Code: —";
        PlayerNameInput.text = "";
   

        hostButton.gameObject.SetActive(true);
        joinButton.gameObject.SetActive(true);
        startGameButton.gameObject.SetActive(false);
        hostButton.interactable = true;
        joinButton.interactable = true;

        SetStatus("Status: Not connected");
    }
    //when we open the lobby, start listening for player list changes, and activate needed buttons.
    private void ShowLobby()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        returnButton.gameObject.SetActive(true);
        //if we're the host, activate the start game button
        bool isServer = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
        startGameButton.gameObject.SetActive(isServer);

        BindSessionManager();
        RebuildPlayerList();

    }
    // If the session manager exists, start listening for player list changes
    private void BindSessionManager()
    {
        sessionManager = SessionManager.Instance != null
            ? SessionManager.Instance
            : FindFirstObjectByType<SessionManager>();

        if (sessionManager == null)
        {
            Debug.LogWarning("SessionManager not found yet. Lobby list will update once it spawns.");
            return;
        }

        // Defensive: avoid double-subscribe
        sessionManager.Players.OnListChanged -= OnPlayersChanged;
        sessionManager.Players.OnListChanged += OnPlayersChanged;
    }
    //stop listening for player list changes
    private void UnbindSessionManager()
    {
        if (sessionManager != null)
        {
            sessionManager.Players.OnListChanged -= OnPlayersChanged;
        }
    }
    //when the player network list changes, we rebuild the UI element
    private void OnPlayersChanged(NetworkListEvent<PlayerData> _)
    {
        RebuildPlayerList();
    }

    // Need to take another look at this func
    private void RebuildPlayerList()
    {
        ClearRows();

        if (sessionManager == null)
        {
            // Try again (useful right after switching panels)
            BindSessionManager();
            if (sessionManager == null)
            {
                SetStatus("Status: Waiting for session...");
                return;
            }
        }

        ulong hostId = NetworkManager.ServerClientId;

        // Copy then sort host first
        var players = new List<PlayerData>(sessionManager.Players.Count);
        for (int i = 0; i < sessionManager.Players.Count; i++)
        {
            players.Add(sessionManager.Players[i]);
        }
        players.Sort((a, b) =>
        {
            bool aHost = a.clientID == hostId;
            bool bHost = b.clientID == hostId;
            if (aHost && !bHost) return -1;
            if (!aHost && bHost) return 1;
            return a.clientID.CompareTo(b.clientID);
        });
        ulong localId = NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId : ulong.MaxValue;

        foreach (var p in players)
        {
            if (rowPrefab == null || listContainer == null) break;

            var row = Instantiate(rowPrefab, listContainer);

            // Name handling: FixedString / string -> ToString() works in both cases
            string name = p.name.ToString();
            string label = $"{name}";

            if (p.clientID == localId) { label += "(you)"; } 

            row.set_name(label);
            spawnedRows.Add(row.gameObject);
        }

        SetStatus($"Status: Lobby - {players.Count} player(s).");
    }
    //clear rows in lobby playerlist
    private void ClearRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null)
                Destroy(spawnedRows[i]);
        }
        spawnedRows.Clear();
    }

    //load the main game scene
    private void OnClickStartGame()
    {
        // Host-only safety
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        SetStatus("Status: Starting game...");

        // TODO: make this fire an event so all clients hide their menus, the event can be invoked when the player presses the menu button as well (hambuger menu in corner)
        corePanel.SetActive(false);

        // IMPORTANT: Session Manager should always be loaded
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Additive);
    }

    // Handles return button. If we connected to relay, disconnect
    private void OnClickReturn()
    {
        if (relay != null)
        {
            relay.Shutdown();  
        }
        UnbindSessionManager();
        ClearRows();
        ShowMainMenu();

    }
    //Handles click join
    private void OnClickJoin()
    {
        hostButton.interactable = false;
        joinButton.interactable = false;

        if (joinSection) joinSection.SetActive(true);
        if (returnButton) returnButton.gameObject.SetActive(true);
        SetStatus("Status: Enter a join code.");

        if (hostButton) hostButton.gameObject.SetActive(false);
        if (joinButton) joinButton.gameObject.SetActive(false);

        PlayerNameInput.gameObject.SetActive(true);
    }
    // Handles click host
    private void OnClickHost()
    {
        SetStatus("Status: Starting host…");
        HostSection.SetActive(true);
        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);

        PlayerNameInput.gameObject.SetActive(true);
        InitLobbyButton.gameObject.SetActive(true);
        returnButton.gameObject.SetActive(true);

    }

    private async void OnClickInitLobby()
    {
        if (PlayerNameInput.text == "")
        {
            SetStatus("Please enter a valid player name");
            return;
        }

        try
        {
            string joinCode = await relay.StartHostAsync(maxClients: 1);
            if (sessionManager == null)
            {
                BindSessionManager();
            }
            sessionManager.RegisterPlayerRpc(PlayerNameInput.text);

            joinCodeLabel.text = $"Join Code: {joinCode}";
            SetStatus("Status: Hosting. Share the join code.");

            returnButton.gameObject.SetActive(true);
            ShowLobby();
        }
        catch (Exception e)
        {
            SetStatus($"Status: Host failed — {ShortMsg(e)}");
            returnButton.gameObject.SetActive(false);
        }



    }
    // Handles when we click 'connect' after entering join code
    private async void OnClickConnect()
    {

        SetStatus("Status: Joining…");
        //here we can also check if there should be any other prohibitions
        if(PlayerNameInput.text == "")
        {
            SetStatus("Please enter a valid player name");
            return;
        }

        try
        {
            string code = joinCodeInput != null ? joinCodeInput.text : "";
            
            await relay.StartClientAsync(code);
            sessionManager.RegisterPlayerRpc(PlayerNameInput.text); 

            SetStatus("Status: Connected / joining completed.");
            returnButton.gameObject.SetActive(false);
            ShowLobby();

        }
        catch (Exception e)
        {
            SetStatus("Status: Join failed — Incorrect Join Code?");
            Debug.Log(ShortMsg(e));
            returnButton.gameObject.SetActive(true);
        }

     
    }
    // When a client connects, update the status label depending on ID
    private void OnClientConnected(ulong clientID)
    {
        var nm = NetworkManager.Singleton;
        if (nm ==  null) return;

        if (nm.IsServer)
            SetStatus($"Status: Client connected (id {clientID}).");
        else
            SetStatus("Status: Connected to host!");

    }
    // When a client disconnects, either update status label or return to mainmenu
    private void OnClientDisconnected(ulong clientID)
    {
        ulong hostId = NetworkManager.ServerClientId;


        var nm = NetworkManager.Singleton;
        if (nm == null) return;

        // Someone else disconnected, and not host. If the host disconnects everyone should be ejected
        if (clientID != nm.LocalClientId && clientID != hostId)
        {
            SetStatus($"Status: Player {clientID} disconnected.");
            return;
        }

        // We disconnected (client lost host / host shutdown / etc.)
        SetStatus("Status: Disconnected.");
        UnbindSessionManager();
        ClearRows();
        ShowMainMenu();
    }



    private void SetStatus(string msg)
    {
        if (statusLabel) statusLabel.text = msg;
        Debug.Log(msg);
    }

    private string ShortMsg(Exception e)
    {
        return e.Message.Replace("\n", " ").Replace("\r", " ");
    }
}
