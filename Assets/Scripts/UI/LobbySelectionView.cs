using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using System;

public class LobbySelectionView : BaseView
{
    public CreateLobbyView createLobbyView;
    public MainMenuView mainMenuView;
    public LobbyListSlot lobbySlotPrefab;
    public Transform noLobbyAvailableText;
    public TMP_InputField codeInputField;
    public Transform slotParent;
    private List<LobbyListSlot> lobbiesSlotLists = new List<LobbyListSlot>();

    private void Start()
    {
        InitUnityServices.Instance.RegisterOnSingnedInAction(OnInitUnityServices);
    }

    private void OnDestroy()
    {
        InitUnityServices.Instance.DeregisterOnSingnedInAction(OnInitUnityServices);
    }

    public override void ShowView(Action action = null)
    {
        base.ShowView();
        SetLobbies();
    }

    public void OnInitUnityServices()
    {
        SetLobbies();
    }

    private async void SetLobbies()
    {
        CleanLobbiesSlot();

        List<Lobby> lobbies = await LobbyManager.Instance.GetLobbiesList();
        noLobbyAvailableText.gameObject.SetActive(lobbies == default);
        if (lobbies == default)
            return;

        foreach (Lobby lobby in lobbies)
        {
            LobbyListSlot slot = Instantiate(lobbySlotPrefab, slotParent);
            slot.SetSlot(lobby.Name, lobby.Id);
            lobbiesSlotLists.Add(slot);
        }
    }

    public void CleanLobbiesSlot()
    {
        if (lobbiesSlotLists.Count == 0)
            return;

        foreach (var slot in lobbiesSlotLists)
        {
            slot.gameObject.SetActive(false);
            Destroy(slot);
        }
        lobbiesSlotLists.Clear();
    }

    public void OnRefreshButtonClick()
    {
        SetLobbies();
    }

    public void OnCreateLobbyButtonClick()
    {
        createLobbyView.ShowView();
        HideView();
    }

    public void OnJoinButtonClick()
    {
        LobbyManager.Instance.JoinLobbyByCodeAsync(codeInputField.textComponent.text);
        HideView();
    }

    public void OnQuickJoinButtonClick()
    {
        LobbyManager.Instance.QuickJoinLobbyAsync();
        HideView();
    }

    public void OnCloseButtonClick()
    {
        mainMenuView.ShowView();
        HideView();
    }
}
