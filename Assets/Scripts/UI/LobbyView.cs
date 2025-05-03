using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using System;
using Unity.Services.Lobbies;
using Unity.Netcode;

public class LobbyView : BaseView
{
    public TMP_Text lobbyNameText;
    public TMP_Text lobbyCodeText;
    public TMP_Text readyButtonText;
    public LobbySelectionView lobbySelectionView;
    public PlayerSlotLobby playerSlotLobby;
    public Transform slotParent;
    public PlayerReady playerReady;
    private Dictionary<string, PlayerSlotLobby> playerSlotsDictionary = new Dictionary<string, PlayerSlotLobby>();
    private bool isReady = false;

    private void Start()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        string id = clientId.ToString();
        if (playerSlotsDictionary.ContainsKey(id))
        {
            playerSlotsDictionary[id].gameObject.SetActive(false);
            Destroy(playerSlotsDictionary[id].gameObject);
            playerSlotsDictionary.Remove(id);
        }
    }

    public void SetData(Lobby lobby)
    {
        ShowView();
        lobbyNameText.text = lobby.Name;
        lobbyCodeText.text = lobby.LobbyCode;
    }

    public void SetReady(bool isReady)
    {
        readyButtonText.text = isReady ? "Not Ready" : "Ready";
        this.isReady = isReady;
    }

    public void OnReadyButtonClick()
    {
        isReady = !isReady;
        readyButtonText.text = isReady ? "Not Ready" : "Ready";
        playerReady.SetPlayerReady(InitUnityServices.Instance.PlayerId, isReady);
    }

    public void SetPlayerNotReady()
    {
        isReady = false;
        readyButtonText.text = "Ready";
    }

    public void OnCloseButtonClick()
    {
        LobbyManager.Instance.LeaveLobby(InitUnityServices.Instance.PlayerId);
        lobbySelectionView.ShowView();
        HideView();
    }

    public async void CreateAndAddPlayerSlot()
    {
        Lobby lobby = await LobbyManager.Instance.SetUpdatedLobbyAsync();
        bool isHost = lobby.HostId == InitUnityServices.Instance.PlayerId;

        foreach (var player in lobby.Players)
        {
            Dictionary<string, PlayerDataObject> playerData = player.Data;
            if (!playerSlotsDictionary.ContainsKey(player.Id))
            {
                PlayerSlotLobby slot = Instantiate(playerSlotLobby, slotParent);
                slot.gameObject.SetActive(true);
                slot.SetSlot(playerData[Constant.PLAYER_NAME].Value, PlayerManager.Instance.IsPlayerReady(player.Id), isHost, player.Id);
                playerSlotsDictionary.Add(player.Id, slot);
                SetReady(PlayerManager.Instance.IsPlayerReady(player.Id));
            }
        }
    }

    public void ClearPlayerSlot()
    {
        foreach (var item in playerSlotsDictionary)
        {
            Destroy(item.Value.gameObject);
        }
        playerSlotsDictionary.Clear();
    }

    public void RemovePlayerSlot(string authId)
    {
        if (playerSlotsDictionary.ContainsKey(authId))
        {
            Destroy(playerSlotsDictionary[authId].gameObject);
            playerSlotsDictionary.Remove(authId);
        }
    }

    public void RemoveAllPlayerSlot()
    {
        foreach (var slot in playerSlotsDictionary)
        {
            Destroy(slot.Value.gameObject);
        }
        playerSlotsDictionary.Clear();
    }

    public void OnPlayerReady()
    {
        CreateAndAddPlayerSlot();
        Dictionary<string, bool> readyData = PlayerManager.Instance.GetPlayerReadyData();
        foreach (var slot in readyData)
        {
            if (playerSlotsDictionary.ContainsKey(slot.Key))
                playerSlotsDictionary[slot.Key].SetReady(slot.Value);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }
}
