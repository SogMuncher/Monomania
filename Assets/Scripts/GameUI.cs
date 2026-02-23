using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using Unity.Netcode;

public class GameUI : MonoBehaviour
{
    // This script is responsible for managing the game UI visibility, and responding to UI buttons

    [Header("Panels")]
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject endTurnPanel;

    [Header("Buttons")]
    [SerializeField] private Button endTurnButton;

    [Header("VFX")]
    [SerializeField] private ParticleSystem startTurnParticles;
    [SerializeField] private ParticleSystem endTurnParticles;

    private void Awake()
    {
        SetVisibilityAll(true);

        endTurnButton.onClick.AddListener(OnClickEndTurn);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Subscribe to turn change event
        TurnManager.Instance.OnTurnChanged += OnTurnChanged;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetVisibilityAll(bool visibility)
    {
        mainCanvas?.SetActive(visibility);
        endTurnPanel?.SetActive(visibility);
    }

    private void OnTurnChanged(int activePlayerIndex)
    {
        Debug.Log("[GameUI] Turn changed, active player index: " + activePlayerIndex);
        // Show end turn panel if it's the player's turn, hide it otherwise
        ulong localClientID = NetworkManager.Singleton.LocalClientId;
        bool isLocalPlayerTurn = SessionManager.Instance.Players[activePlayerIndex].clientID == localClientID;

        SetVisibilityAll(isLocalPlayerTurn);

        if (isLocalPlayerTurn)
        {
            startTurnParticles?.Play();
        }
        else
        {
            endTurnParticles?.Play();
        }
    }

    private void OnClickEndTurn()
    {
        Debug.Log("[GameUI] End Turn button clicked");
        TurnManager.Instance.Server_EndTurn_Rpc();
    }
}
