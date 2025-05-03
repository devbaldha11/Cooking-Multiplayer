using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 spawnPosition = new Vector3(0, 1, 0);

    public static CustomNetworkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void HandleServerStarted()
    {
        if (IsServer)
        {
            SpawnPlayer(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        GameObject playerInstance = Instantiate(playerPrefab, GetSpawnPoint(), Quaternion.identity);

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private Vector3 GetSpawnPoint()
    {
        return spawnPosition;
    }

    public void SpawnPlayerManually()
    {
        if (IsServer)
        {
            SpawnPlayer(NetworkManager.ServerClientId);
        }
        else
        {
            Debug.LogWarning("Cannot spawn player: Not running as server");
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }
}