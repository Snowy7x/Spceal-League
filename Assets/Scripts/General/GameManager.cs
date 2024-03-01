using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameObject playerPrefab;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        base.OnNetworkSpawn();
    }

    private void OnDisable()
    {
        try
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        }catch{}
    }

    private void OnSceneLoaded(string scenename, LoadSceneMode loadscenemode, List<ulong> clientscompleted, List<ulong> clientstimedout)
    {
        if (scenename == "Game" && NetworkManager.Singleton.IsHost)
        {
            Debug.Log($"Spawning players amount: {clientscompleted.Count}");
            foreach (var client in clientscompleted)
            {
                Debug.Log($"Spawning player for client {client}");
                SpawnPlayer(client);
            }
        }
    }
    
    


    /*public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc(NetworkManager.LocalClientId);
        base.OnNetworkSpawn();
    }*/

    public void SpawnPlayer(ulong clientId)
    {
        var player = Instantiate(playerPrefab);
        player.transform.position = SpawnManager.Instance.GetSpawnPoint().position;
        player.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }
}