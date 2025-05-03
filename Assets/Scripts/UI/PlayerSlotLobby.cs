using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSlotLobby : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text readyText;
    [SerializeField] private GameObject kickButton;

    private string authId;

    public void SetSlot(string playerName, bool isReady, bool isHost, string authId = default)
    {
        if (isHost)
            this.authId = authId;
        playerNameText.text = playerName;
        readyText.text = isReady ? "Ready" : "Not Ready";
        kickButton.gameObject.SetActive(isHost && (LobbyManager.Instance.MyLobby.HostId != authId));
    }

    public void SetReady(bool isReady)
    {
        readyText.text = isReady ? "Ready" : "Not Ready";
    }

    public void OnKickPlayerButtonClick()
    {
        Debug.Log("Authid " + authId);
        LobbyManager.Instance.KickPlayer(authId);
    }
}
