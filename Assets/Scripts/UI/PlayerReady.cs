using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerReady : NetworkBehaviour
{
    [SerializeField] private Player playerPrefab;
    [SerializeField] private LobbyView lobbyView;
    [SerializeField] private LobbySelectionView lobbySelectionView;
    [SerializeField] private MessageView messageView;

    private Dictionary<string, bool> playerReadyDictionary = new Dictionary<string, bool>();
    private bool isAllPlayerReady;

    public bool IsAllPlayerReady => isAllPlayerReady;

    public void InvokeOnPlayerReadyAction()
    {
        lobbyView.OnPlayerReady();
    }

    public void SetPlayerReady(string authId, bool isReady)
    {
        SetPlayerReadyServerRpc(authId, isReady);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(string authId, bool isReady, ServerRpcParams serverRpcParams = default)
    {
        PlayerManager.Instance.SetPlayerReady(authId, serverRpcParams.Receive.SenderClientId, isReady);
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId.ToString()] = isReady;

        bool isAllPlayerReady = true;

        foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(id.ToString()) || (playerReadyDictionary.ContainsKey(id.ToString()) && playerReadyDictionary[id.ToString()] == false))
            {
                isAllPlayerReady = false;
                break;
            }
        }

        if (isAllPlayerReady)
        {
            this.isAllPlayerReady = true;
            OnLoadSceneStartClientRpc();
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }

    public void ResetPlayerReady()
    {
        foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            PlayerManager.Instance.SetPlayerReady(PlayerManager.Instance.GetAuthIdFromClientId(id), id, false);
            playerReadyDictionary[id.ToString()] = false;
        }
    }

    [ClientRpc]
    private void OnLoadSceneStartClientRpc()
    {
        lobbyView.HideView();
        messageView.ShowView(Constant.LOADING);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (var id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Player player = Instantiate(playerPrefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
        }
        MultiplayerManager.Instance.StartCountdown();
        OnLoadSceneCompleteClientRpc();
    }

    [ClientRpc]
    private void OnLoadSceneCompleteClientRpc()
    {
        messageView.HideView();
        lobbyView.HideView();
        lobbySelectionView.HideView();
    }
}
