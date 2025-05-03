using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyListSlot : MonoBehaviour
{
    public TMP_Text lobbyNameText;

    private string lobbyId;

    public void SetSlot(string lobbyName, string lobbyId)
    {
        lobbyNameText.text = lobbyName;
        this.lobbyId = lobbyId;
        gameObject.SetActive(true);
    }

    public void OnJoinButtonClick()
    {
        LobbyManager.Instance.JoinLobbyByIdAsync(lobbyId);
    }
}
