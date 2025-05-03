using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class TestingNeetcode : MonoBehaviour
{
    [SerializeField] private Transform ui;

    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Hosted");
        Hide();
    }

    public void Client()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client Join");
        Hide();
    }

    private void Hide()
    {
        ui.gameObject.SetActive(false);
    }
}
