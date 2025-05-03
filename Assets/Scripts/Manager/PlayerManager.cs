using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Collections;

public class PlayerManager : NetworkManager<PlayerManager>
{
    [SerializeField] MessageView messageView;
    [SerializeField] LobbyView lobbyView;
    [SerializeField] LobbySelectionView lobbySelectionView;
    [SerializeField] private PlayerReady playerReady;

    private NetworkList<PlayerNetworkData> playerData;
    private Dictionary<ulong, Player> playerCache = new Dictionary<ulong, Player>();

    public override void Awake()
    {
        base.Awake();

        playerData = new NetworkList<PlayerNetworkData>();
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
        }
    }

    public override void OnNetworkSpawn()
    {
        playerData.OnListChanged += OnPlayerDataChanged;

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        playerData.OnListChanged -= OnPlayerDataChanged;

        base.OnNetworkDespawn();
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
        }
        base.OnDestroy();
    }

    public Dictionary<string, bool> GetPlayerReadyData()
    {
        Dictionary<string, bool> readyData = new Dictionary<string, bool>();
        foreach (var item in playerData)
        {
            readyData.Add(item.authId.ToString(), item.isReady);
        }
        return readyData;
    }

    public string GetAuthIdFromClientId(ulong clientId)
    {
        foreach (var item in playerData)
        {
            if (item.clientId == clientId)
                return item.authId.ToString();
        }
        return "";
    }

    public ulong GetClientIdFromAuthId(string authId)
    {
        foreach (var item in playerData)
        {
            if (item.authId == authId)
                return item.clientId;
        }
        return ulong.MaxValue;
    }

    public void RemoveAllPlayerData()
    {
        foreach (var item in playerData)
        {
            playerData.Remove(item);
            RemovePlayerSlotClientRpc(item.authId.ToString());
        }
    }

    public void RemovePlayerData(string authId)
    {
        foreach (var item in playerData)
        {
            if (item.authId.ToString() == authId)
            {
                playerData.Remove(item);
                RemovePlayerSlotClientRpc(item.authId.ToString());
                break;
            }
        }
    }

    [ServerRpc]
    public void DespawnPlayersServerRpc()
    {
        foreach (var player in playerCache)
        {
            NetworkObjectReference networkObjectReference = player.Value.NetworkObject;
            networkObjectReference.TryGet(out NetworkObject itemNetworkObject);
            itemNetworkObject.Despawn(true);
        }
    }

    [ClientRpc]
    private void RemovePlayerSlotClientRpc(string authId)
    {
        RemovePlayerFromLobby(authId);
    }

    private void OnPlayerConnected(ulong clientId)
    {
        messageView.HideView();
        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;
        AddPlayerDataServerRpc(clientId, InitUnityServices.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerDataServerRpc(ulong clientId, string authId)
    {
        playerData.Add(new PlayerNetworkData()
        {
            clientId = clientId,
            isReady = false,
            authId = authId
        });
    }

    private void OnPlayerDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            lobbyView.RemoveAllPlayerSlot();
            lobbyView.HideView();
            lobbySelectionView.ShowView();
            messageView.ShowView(Constant.DISCONNECTED);
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Shutdown");
        }

        if (!NetworkManager.Singleton.IsServer)
            return;

        RemovePlayerData(GetAuthIdFromClientId(clientId));
    }

    private void RemovePlayerFromLobby(string authId)
    {
        lobbyView.RemovePlayerSlot(authId);
    }

    private void OnPlayerDataChanged(NetworkListEvent<PlayerNetworkData> changeEvent)
    {
        if (playerCache.Count != playerData.Count)
            UpdateLocalCache();
        if (playerData.Count < playerCache.Count)
            return;
        playerReady.InvokeOnPlayerReadyAction();
    }

    private List<ulong> GetConnectedClientIds()
    {
        List<ulong> ids = new List<ulong>();
        for (int i = 0; i < playerData.Count; i++)
        {
            ids.Add(playerData[i].clientId);
        }
        return ids;
    }

    public void UpdateLocalCache()
    {
        var activeIds = GetConnectedClientIds();
        playerCache.Clear();

        var players = FindObjectsOfType<Player>();

        foreach (var player in players)
        {
            if (activeIds.Contains(player.OwnerClientId))
            {
                playerCache[player.OwnerClientId] = player;
                player.SetPlayerNameText(LobbyManager.Instance.GetPlayerName(player.OwnerClientId));
            }
        }
    }

    public bool IsPlayerReady(string authId)
    {
        for (int i = 0; i < playerData.Count; i++)
        {
            if (playerData[i].authId.ToString() == authId)
                return playerData[i].isReady;
        }
        return false;
    }

    public void SetPlayerReady(string authId, ulong clientId, bool isReady)
    {
        for (int i = 0; i < playerData.Count; i++)
        {
            if (playerData[i].clientId == clientId)
            {
                playerData[i] = new PlayerNetworkData()
                {
                    clientId = clientId,
                    isReady = isReady,
                    authId = authId
                };
                break;
            }
        }
    }

    public static Player GetPlayer(ulong clientId)
    {
        return Instance.playerCache.TryGetValue(clientId, out var player) ? player : null;
    }

    public struct PlayerNetworkData : IEquatable<PlayerNetworkData>, INetworkSerializable
    {
        public ulong clientId;
        public FixedString512Bytes authId;
        public bool isReady;

        public bool Equals(PlayerNetworkData other)
        {
            return clientId == other.clientId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref authId);
            serializer.SerializeValue(ref isReady);
        }
    }
}
