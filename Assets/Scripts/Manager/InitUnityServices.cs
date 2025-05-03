using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class InitUnityServices : Manager<InitUnityServices>
{
    public List<Action> OnSingnedInAction = new List<Action>();
    private string playerId;

    public string PlayerId => playerId;

    public async void Init()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += OnSignedIn;
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public void RegisterOnSingnedInAction(Action action)
    {
        if (!OnSingnedInAction.Contains(action))
            OnSingnedInAction.Add(action);
    }

    public void DeregisterOnSingnedInAction(Action action)
    {
        if (OnSingnedInAction.Contains(action))
            OnSingnedInAction.Remove(action);
    }

    private void OnSignedIn()
    {
        playerId = AuthenticationService.Instance.PlayerId;
        Debug.LogError("Signed In");
    }
}
