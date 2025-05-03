using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerManager : NetworkManager<MultiplayerManager>
{
    public GameState gameState;

    [SerializeField] private Cinemachine.CinemachineVirtualCamera cinemachine;
    [SerializeField] private float gameOverTime;
    [SerializeField] private TimeLeftView timeLeftView;
    [SerializeField] private StartCountdownView startCountdownView;

    private NetworkVariable<int> startCountDownTimeNetworkVariable = new NetworkVariable<int>();
    private NetworkVariable<int> gameOverTimeNetworkVariable = new NetworkVariable<int>();
    private NetworkVariable<int> gameStateNetworkVariable = new NetworkVariable<int>();

    public Cinemachine.CinemachineVirtualCamera Cinemachine => cinemachine;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        startCountDownTimeNetworkVariable.OnValueChanged += OnCountdownTimeValueChange;
        gameOverTimeNetworkVariable.OnValueChanged += OnGameOverTimeValueChange;
        gameStateNetworkVariable.OnValueChanged += OnGameStateValueChange;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        startCountDownTimeNetworkVariable.OnValueChanged -= OnCountdownTimeValueChange;
        gameOverTimeNetworkVariable.OnValueChanged -= OnGameOverTimeValueChange;
        gameStateNetworkVariable.OnValueChanged -= OnGameStateValueChange;
    }

    public void SetLookAtCamera(Transform transform)
    {
        cinemachine.LookAt = transform;
    }

    public void SpawnItem(ItemID itemID, NetworkObjectReference counterNetworkObjectReference, ulong playerID = ulong.MaxValue)
    {
        SpawnItemServerRpc((int)itemID, counterNetworkObjectReference, playerID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnItemServerRpc(int id, NetworkObjectReference counterNetworkObjectReference, ulong playerID)
    {
        Item item = Instantiate(ResourceManager.Instance.itemSO.GetItemPrefabByItemID((ItemID)id));

        NetworkObject networkObject = item.NetworkObject;
        networkObject.Spawn(true);

        counterNetworkObjectReference.TryGet(out NetworkObject counterNetworkObject);
        counterNetworkObject.TryGetComponent(out BaseCounter baseCounter);
        baseCounter?.OnItemSpawn(playerID, item);
    }

    public void DespawnItem(NetworkObjectReference itemNetworkObjectReference, ulong playerID = ulong.MaxValue)
    {
        DespawnItemServerRpc(itemNetworkObjectReference, playerID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnItemServerRpc(NetworkObjectReference itemNetworkObjectReference, ulong playerID)
    {
        itemNetworkObjectReference.TryGet(out NetworkObject itemNetworkObject);
        itemNetworkObject.Despawn(true);
        if (playerID != ulong.MaxValue)
            PlayerManager.GetPlayer(playerID).MyItem = null;
    }

    public void StartCountdown()
    {
        gameState = GameState.GamePlaying;
        StartCoroutine(StartCountdownCO());
        StartCountdownClientRpc();
    }

    [ClientRpc]
    public void StartCountdownClientRpc()
    {
        PlayerManager.Instance.UpdateLocalCache();
        startCountdownView.ShowView();
    }

    public IEnumerator StartCountdownCO()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        int i = 3;
        while (i >= 0)
        {
            startCountDownTimeNetworkVariable.Value = i;
            yield return waitForSeconds;
            i--;
        }
        if (IsServer)
        {
            OrderManager.Instance.StartGeneratingOrder();
            StartCoroutine(StartGameOverTimer());
        }
        yield return null;
    }

    public IEnumerator StartGameOverTimer()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f);
        int i = 0;
        while (i <= gameOverTime)
        {
            gameOverTimeNetworkVariable.Value = i;
            yield return waitForSeconds;
            i++;
        }
        OnGameOverClientRpc();
        LobbyManager.Instance.ResetPlayerData();
        PlayerManager.Instance.DespawnPlayersServerRpc();
        NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName("Gameplay"));
        yield return null;
    }

    [ClientRpc]
    public void OnGameOverClientRpc()
    {
        LobbyManager.Instance.OnGameOver(OrderManager.Instance.RecipeDelivered);
    }

    private void OnCountdownTimeValueChange(int previous, int current)
    {
        startCountdownView.SetCountdownText(current.ToString());
        if (current < 1)
            startCountdownView.HideView();
    }

    private void OnGameOverTimeValueChange(int previous, int current)
    {
        timeLeftView.SetFillAmount(current / gameOverTime);
    }

    private void OnGameStateValueChange(int previous, int current)
    {
        gameState = (GameState)current;
    }
}

public enum GameState
{
    WaitingForPlayers,
    GamePlaying,
    GameOver
}
