using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class MainMenuUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RelayConnector relay;

    [Header("UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    [SerializeField] private GameObject joinSection;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private Button connectButton;

    [SerializeField] private TMP_Text joinCodeLabel;
    [SerializeField] private TMP_Text statusLabel;

    private void Reset()
    {
        relay = FindFirstObjectByType<RelayConnector>();
    }

    private void Awake()
    {
        // Default UI state
        if (joinSection) joinSection.SetActive(false);
        if (joinCodeLabel) joinCodeLabel.text = "Join Code: —";
        SetStatus("Status: Not connected");

        hostButton.onClick.AddListener(OnClickHost);
        joinButton.onClick.AddListener(OnClickJoin);
        connectButton.onClick.AddListener(OnClickConnect);

        // Optional: react to NGO connection events
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClickJoin()
    {
        if (joinSection) joinSection.SetActive(true);
        SetStatus("Status: Enter a join code.");

        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
    }

    private async void OnClickHost()
    {
        DisableMenuButtons();
        SetStatus("Status: Starting host…");

        try
        {
            // 2-player demo => 1 other client can join
            string joinCode = await relay.StartHostAsync(maxClients: 1);

            if (joinCodeLabel) joinCodeLabel.text = $"Join Code: {joinCode}";
            SetStatus("Status: Hosting. Share the join code.");
        }
        catch (Exception e)
        {
            SetStatus($"Status: Host failed — {ShortMsg(e)}");
            EnableMenuButtons();
        }
    }

    private async void OnClickConnect()
    {
        DisableMenuButtons();
        SetStatus("Status: Joining…");

        try
        {
            string code = joinCodeInput != null ? joinCodeInput.text : "";
            await relay.StartClientAsync(code);

            SetStatus("Status: Joining requested…");
        }
        catch (Exception e)
        {
            SetStatus($"Status: Join failed — {ShortMsg(e)}");
            EnableMenuButtons();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // This event fires on host and clients.
        if (NetworkManager.Singleton.IsServer)
        {
            SetStatus($"Status: Client connected (id {clientId}).");
        }
        else
        {
            SetStatus("Status: Connected to host!");
        }

        // Once connected, you might hide the menu if you want:
        // gameObject.SetActive(false);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        SetStatus("Status: Disconnected.");
        EnableMenuButtons();
    }

    private void DisableMenuButtons()
    {
        hostButton.interactable = false;
        joinButton.interactable = false;
        connectButton.interactable = false;
    }

    private void EnableMenuButtons()
    {
        hostButton.interactable = true;
        joinButton.interactable = true;
        connectButton.interactable = true;
    }

    private void SetStatus(string msg)
    {
        if (statusLabel) statusLabel.text = msg;
        Debug.Log(msg);
    }

    private string ShortMsg(Exception e)
    {
        // trims long stack-trace-y messages into something readable
        return e.Message.Replace("\n", " ").Replace("\r", " ");
    }
}
