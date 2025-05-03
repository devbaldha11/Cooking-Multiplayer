using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
using System;
using Unity.Netcode;

public class LobbyManager : NetworkManager<LobbyManager>
{
    [SerializeField] private CreateLobbyView createLobbyView;
    [SerializeField] private LobbySelectionView lobbySelectionView;
    [SerializeField] private LobbyView lobbyView;
    [SerializeField] private MessageView messageView;
    [SerializeField] private PlayerReady playerReady;
    [SerializeField] private GameOverView gameOverView;

    private Lobby lobby;

    public Lobby MyLobby => lobby;

    public string MyLobbyId => lobby.Id;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
        }
    }

    public async void CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate)
    {
        try
        {
            messageView.ShowView(Constant.CREATING_LOBBY, false);
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = new Unity.Services.Lobbies.Models.Player(InitUnityServices.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        { Constant.PLAYER_NAME , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerPrefsHelper.Instance.GetString(Constant.PLAYER_NAME) )},
                    }
                }
            };
            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            SubscribeToLobbyEvents();
            Allocation allocation = await CreateAllocationAsync(maxPlayers);
            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCallback;
                NetworkManager.Singleton.Shutdown();
            }
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            string joinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>()
                {
                    { Constant.RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                }
            });
            NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCallback;
            NetworkManager.Singleton.StartHost();

            StartCoroutine(HeartbeatLobbyCO(lobby.Id));
            lobbyView.SetData(lobby);
        }
        catch (LobbyServiceException e)
        {
            messageView.ShowView(Constant.FAILED_TO_CREATE);
            createLobbyView.ShowView();
        }
        catch (RelayServiceException e)
        {
            messageView.ShowView(Constant.FAILED_TO_CREATE);
            createLobbyView.ShowView();
        }
    }

    public async void JoinLobbyByCodeAsync(string code)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions()
            {
                Player = new Unity.Services.Lobbies.Models.Player(InitUnityServices.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        { Constant.PLAYER_NAME , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerPrefsHelper.Instance.GetString(Constant.PLAYER_NAME) )},
                    }
                }
            };
            if (NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();

            messageView.ShowView(Constant.CONNECTING_LOBBY, false);
            lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
            SubscribeToLobbyEvents();
            JoinAllocation allocation = await JoinAllocationAsync(lobby.Data[Constant.RELAY_JOIN_CODE].Value);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            NetworkManager.Singleton.StartClient();
            lobbyView.SetData(lobby);
        }
        catch (LobbyServiceException e)
        {
            messageView.ShowView(Constant.FAILED_TO_CONNECT);
            lobbySelectionView.ShowView();
        }
        catch (RelayServiceException e)
        {
            messageView.ShowView(Constant.FAILED_TO_CONNECT);
            lobbySelectionView.ShowView();
        }
    }

    public async void JoinLobbyByIdAsync(string id)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions()
            {
                Player = new Unity.Services.Lobbies.Models.Player(InitUnityServices.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        { Constant.PLAYER_NAME , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerPrefsHelper.Instance.GetString(Constant.PLAYER_NAME) )},
                    }
                }
            };
            if (NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();

            messageView.ShowView(Constant.CONNECTING_LOBBY, false);
            lobby = await LobbyService.Instance.JoinLobbyByIdAsync(id, joinLobbyByIdOptions);
            SubscribeToLobbyEvents();
            JoinAllocation allocation = await JoinAllocationAsync(lobby.Data[Constant.RELAY_JOIN_CODE].Value);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            NetworkManager.Singleton.StartClient();
            lobbyView.SetData(lobby);
        }
        catch (LobbyServiceException e)
        {
            messageView.ShowView(Constant.FAILED_TO_CONNECT);
            lobbySelectionView.ShowView();
        }
        catch (RelayServiceException e)
        {
            messageView.ShowView(Constant.FAILED_TO_CONNECT);
            lobbySelectionView.ShowView();
        }
    }

    public async void QuickJoinLobbyAsync()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions()
            {
                Player = new Unity.Services.Lobbies.Models.Player(InitUnityServices.Instance.PlayerId)
                {
                    Data = new Dictionary<string, PlayerDataObject>()
                    {
                        { Constant.PLAYER_NAME , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerPrefsHelper.Instance.GetString(Constant.PLAYER_NAME) )},
                    }
                }
            };
            if (NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();

            messageView.ShowView(Constant.CONNECTING_LOBBY, false);
            lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            SubscribeToLobbyEvents();
            JoinAllocation allocation = await JoinAllocationAsync(lobby.Data[Constant.RELAY_JOIN_CODE].Value);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
            NetworkManager.Singleton.StartClient();
            lobbyView.SetData(lobby);
        }
        catch (LobbyServiceException e)
        {
            messageView.ShowView(Constant.FAILED_TO_CONNECT);
            lobbySelectionView.ShowView();
        }
        catch (RelayServiceException e)
        {
            messageView.ShowView(Constant.FAILED_TO_CONNECT);
            lobbySelectionView.ShowView();
        }
    }

    public async Task<List<Lobby>> GetLobbiesList()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>()
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>()
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            return queryResponse.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    public void LeaveLobby(string playerId)
    {
        if (NetworkManager.Singleton.IsServer)
            DeleteLobbyServerRpc(playerId);
        else
            RemovePlayerServerRpc(playerId);
    }

    public void KickPlayer(string playerId)
    {
        RemovePlayerServerRpc(playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeleteLobbyServerRpc(string playerId)
    {
        DeleteLobbyAsync(playerId);
    }

    public async void DeleteLobbyAsync(string playerId)
    {
        try
        {
            PlayerManager.Instance.RemoveAllPlayerData();
            await LobbyService.Instance.DeleteLobbyAsync(MyLobbyId);
            lobbyView.RemoveAllPlayerSlot();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ConnectionApprovalCallback;
            NetworkManager.Singleton.Shutdown();
            Debug.Log("delete lobby Shutdown");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("shutdown not possible " + e);
        }
    }

    public void ResetPlayerData()
    {
        playerReady.ResetPlayerReady();
    }

    public void OnGameOver(int deliveredRecipe)
    {
        gameOverView.SetData(deliveredRecipe);
        lobbyView.SetPlayerNotReady();
        lobbyView.ShowView();
    }

    private async void SubscribeToLobbyEvents()
    {
        var callbacks = new LobbyEventCallbacks();
        callbacks.LobbyDeleted += Callbacks_LobbyDeleted;
        var m_LobbyEvents = await Lobbies.Instance.SubscribeToLobbyEventsAsync(MyLobbyId, callbacks);
    }

    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (!playerReady.IsAllPlayerReady && NetworkManager.Singleton.ConnectedClientsIds.Count < MyLobby.MaxPlayers)
            connectionApprovalResponse.Approved = true;
        else
            connectionApprovalResponse.Approved = false;
    }

    private void Callbacks_LobbyDeleted()
    {
        lobbyView.HideView();
        lobbySelectionView.ShowView();
        messageView.ShowView(Constant.DISCONNECTED);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerServerRpc(string playerId)
    {
        ulong clientId = PlayerManager.Instance.GetClientIdFromAuthId(playerId);
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };
        UpdateUIDisconnectedClientRpc(clientRpcParams);
        LobbyService.Instance.RemovePlayerAsync(MyLobbyId, playerId);
        NetworkManager.Singleton.DisconnectClient(PlayerManager.Instance.GetClientIdFromAuthId(playerId));
        PlayerManager.Instance.RemovePlayerData(playerId);
    }

    [ClientRpc]
    public void UpdateUIDisconnectedClientRpc(ClientRpcParams clientRpcParam)
    {
        lobbyView.ClearPlayerSlot();
        lobbyView.HideView();
        lobbySelectionView.ShowView();
    }

    public string GetPlayerName(ulong clientId)
    {
        string authId = PlayerManager.Instance.GetAuthIdFromClientId(clientId);
        foreach (var player in lobby.Players)
        {
            if (player.Id == authId)
            {
                return player.Data[Constant.PLAYER_NAME].Value;
            }
        }
        return "";
    }

    public async Task<Lobby> SetUpdatedLobbyAsync()
    {
        try
        {
            lobby = await LobbyService.Instance.GetLobbyAsync(MyLobbyId);
            return lobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return lobby;
        }
    }

    private async Task<Allocation> CreateAllocationAsync(int maxPlayers)
    {
        try
        {
            return await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinAllocationAsync(string joinCode)
    {
        try
        {
            return await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            return await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    private IEnumerator HeartbeatLobbyCO(string lobbyId)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(25);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    private void OnPlayerConnected(ulong clientId)
    {

    }

    private void OnPlayerDisconnected(ulong clientId)
    {

    }
}
