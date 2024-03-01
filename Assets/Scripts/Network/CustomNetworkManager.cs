using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class CustomNetworkManager : MonoBehaviour
{
    public static CustomNetworkManager Instance;
    private Lobby _connectedLobby;

    private UnityTransport _transport;
    private const string JoinCodeKey = "j";
    private string _playerId;
    private event Action<Lobby> OnLobbyJoined;
    private event Action<Lobby> OnLobbyCreated;

    private void Awake()
    {
        _transport = FindObjectOfType<UnityTransport>();
        if (_transport == null)
        {
            Debug.LogError("No transport found");
        }
        
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task Start()
    {
        await Authenticate();
        Debug.Log("Logged in as: " + AuthenticationService.Instance.PlayerId);
        
    }
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("K pressed");
            StartGame();
        }
    }

    public async void CreateOrJoinLobby(int gameMode = 1)
    {
        //await Authenticate();

        _connectedLobby = await QuickJoinLobby() ?? await CreateLobby();
    }

    private async Task Authenticate()
    {
        var option = new InitializationOptions();
/*#if UNITY_EDITOR
        var mppmTag = CurrentPlayer.ReadOnlyTag();
        if (mppmTag != null)
        {
            Debug.Log($"Found MPPM tag: {mppmTag}");
            option.SetProfile(mppmTag);
            
        }
        else
        {
            option.SetProfile("Player" + Random.Range(1, 1000));
        }

        Debug.Log($"No MPPM tag found");
#else
          option.SetProfile("Player" + Random.Range(1, 1000));
#endif*/
        await UnityServices.InitializeAsync(option);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        _playerId = AuthenticationService.Instance.PlayerId;
    }

    private async Task<Lobby> QuickJoinLobby() {
        try {
            var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();

            var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

            SetTransformAsClient(a);

            NetworkManager.Singleton.StartClient();
            return lobby;
        }
        catch (Exception e) {
            Debug.Log($"No lobbies available via quick join");
            return null;
        }
    }

    private async Task<Lobby> CreateLobby() {
        try {
            const int maxPlayers = 100;

            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            var options = new CreateLobbyOptions {
                Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            };
            var lobby = await Lobbies.Instance.CreateLobbyAsync("Useless Lobby Name", maxPlayers, options);

            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

            _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

            NetworkManager.Singleton.StartHost();
            return lobby;
        }
        catch (Exception e) {
            Debug.LogFormat("Failed creating a lobby");
            return null;
        }
    }
    
    private void SetTransformAsClient(JoinAllocation a) {
        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
    }

    private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds) {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while (true) {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnDestroy() {
        try {
            StopAllCoroutines();
            // todo: Add a check to see if you're host
            if (_connectedLobby != null) {
                if (_connectedLobby.HostId == _playerId) Lobbies.Instance.DeleteLobbyAsync(_connectedLobby.Id);
                else Lobbies.Instance.RemovePlayerAsync(_connectedLobby.Id, _playerId);
            }
        }
        catch (Exception e) {
            Debug.Log($"Error shutting down lobby: {e}");
        }
    }

    private void StartGame()
    {
        if (_connectedLobby != null && _connectedLobby.Players.Count > 0)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        } 
    }
}
