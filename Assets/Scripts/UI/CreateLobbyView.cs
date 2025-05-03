using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateLobbyView : BaseView
{
    public LobbySelectionView lobbySelectionView;
    public Toggle publicToggle;
    public Toggle privateToggle;
    public Toggle twoPlayersToggle;
    public Toggle threePlayersToggle;
    public Toggle fourPlayersToggle;
    public TMP_InputField lobbyNameInputField;
    public bool isPrivateLobby;
    public int maxPlayers;

    public void Start()
    {
        SetDefaultValues();
    }

    public void SetDefaultValues()
    {
        publicToggle.isOn = true;
        fourPlayersToggle.isOn = true;
        isPrivateLobby = false;
        maxPlayers = 4;
    }

    public void OnVisibilityToggleValueChange()
    {
        if (privateToggle.isOn)
            isPrivateLobby = true;
        else
            isPrivateLobby = false;
    }

    public void OnMaxPlayersToggleValueChange()
    {
        if (twoPlayersToggle.isOn)
            maxPlayers = 2;
        else if (threePlayersToggle.isOn)
            maxPlayers = 3;
        else
            maxPlayers = 4;
    }

    public void OnCreateLobbyButtonClick()
    {
        if (lobbyNameInputField.textComponent.text == null)
        {
            Debug.Log("Please enter lobby name");
            return;
        }
        LobbyManager.Instance.CreateLobbyAsync(lobbyNameInputField.textComponent.text, maxPlayers, isPrivateLobby);
        HideView();
    }

    public void OnCloseButtonClick()
    {
        lobbySelectionView.ShowView();
        HideView();
    }
}
