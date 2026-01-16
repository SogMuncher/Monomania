using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class RelayConnector : MonoBehaviour
{
    [Header("NGO")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private UnityTransport transport;

    public bool IsReady { get; private set; }

    private void Reset()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        transport = FindFirstObjectByType<UnityTransport>();
    }

    private async void Awake()
    {
        if (networkManager == null)
            networkManager = NetworkManager.Singleton ?? FindFirstObjectByType<NetworkManager>();

        if (transport == null)
            transport = FindFirstObjectByType<UnityTransport>();

        await InitializeServicesAsync();
    }

    public async Task InitializeServicesAsync()
    {
        if (IsReady) return;

        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            IsReady = true;
            Debug.Log($"UGS ready. PlayerId={AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            IsReady = false;
            Debug.LogError($"UGS init failed: {e}");
        }
    }

    /// <summary>
    /// Version-tolerant: try to read RelayServer.Host (if the property exists),
    /// otherwise fall back to RelayServer.IpV4 (if present).
    /// </summary>
    private static string GetRelayHostOrIp(object relayServer)
    {
        if (relayServer == null) return string.Empty;

        // Try "Host" (newer versions)
        var hostProp = relayServer.GetType().GetProperty("Host", BindingFlags.Public | BindingFlags.Instance);
        if (hostProp != null)
        {
            var host = hostProp.GetValue(relayServer) as string;
            if (!string.IsNullOrWhiteSpace(host))
                return host;
        }

        // Fall back to "IpV4" (older versions commonly have this)
        var ipV4Prop = relayServer.GetType().GetProperty("IpV4", BindingFlags.Public | BindingFlags.Instance);
        if (ipV4Prop != null)
        {
            var ipV4 = ipV4Prop.GetValue(relayServer) as string;
            if (!string.IsNullOrWhiteSpace(ipV4))
                return ipV4;
        }

        // As a last resort, try "IpV6" if present
        var ipV6Prop = relayServer.GetType().GetProperty("IpV6", BindingFlags.Public | BindingFlags.Instance);
        if (ipV6Prop != null)
        {
            var ipV6 = ipV6Prop.GetValue(relayServer) as string;
            if (!string.IsNullOrWhiteSpace(ipV6))
                return ipV6;
        }

        return string.Empty;
    }

    private static int GetRelayPort(object relayServer)
    {
        if (relayServer == null) return 0;

        var portProp = relayServer.GetType().GetProperty("Port", BindingFlags.Public | BindingFlags.Instance);
        if (portProp != null)
        {
            object value = portProp.GetValue(relayServer);
            if (value is int i) return i;
            if (value is ushort us) return us;
        }

        return 0;
    }

    /// <summary>
    /// Starts a Host (server + local client) and returns the Relay join code.
    /// maxClients = number of *other* players allowed to join (excluding host).
    /// </summary>
    public async Task<string> StartHostAsync(int maxClients)
    {
        await InitializeServicesAsync();
        if (!IsReady) throw new Exception("Services not ready.");

        if (networkManager != null && networkManager.IsListening)
            throw new Exception("Network already running.");

        try
        {
            // Some versions interpret this parameter differently.
            // If you run into "lobby full" issues, change back to maxClients.
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxClients + 1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Get endpoint in a version-tolerant way
            string hostOrIp = GetRelayHostOrIp(allocation.RelayServer);
            ushort port = (ushort)GetRelayPort(allocation.RelayServer);

            Debug.Log($"[Relay Host] endpoint='{hostOrIp}:{port}' joinCode={joinCode}");

            transport.SetHostRelayData(
                hostOrIp,
                port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                isSecure: false
            );

            if (!networkManager.StartHost())
                throw new Exception("StartHost() failed.");

            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"StartHostAsync failed: {e}");
            throw;
        }
    }

    public async Task StartClientAsync(string joinCode)
    {
        await InitializeServicesAsync();
        if (!IsReady) throw new Exception("Services not ready.");

        if (networkManager != null && networkManager.IsListening)
            throw new Exception("Network already running.");

        if (string.IsNullOrWhiteSpace(joinCode))
            throw new Exception("Join code is empty.");

        joinCode = joinCode.Trim().ToUpperInvariant();

        try
        {
            Debug.Log($"[Relay Client] Attempting JoinAllocation with code='{joinCode}'");

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            string hostOrIp = GetRelayHostOrIp(joinAllocation.RelayServer);
            ushort port = (ushort)GetRelayPort(joinAllocation.RelayServer);

            Debug.Log($"[Relay Client] endpoint='{hostOrIp}:{port}'");

            transport.SetClientRelayData(
                hostOrIp,
                port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                //TODO
                //Should not be false on production build!
                isSecure: false
            );

            if (!networkManager.StartClient())
                throw new Exception("StartClient() failed.");
        }
        catch (Exception e)
        {
            Debug.LogError($"StartClientAsync failed: {e}");
            throw;
        }
    }

    public void Shutdown()
    {
        if (networkManager != null && networkManager.IsListening)
            networkManager.Shutdown();
    }
}
