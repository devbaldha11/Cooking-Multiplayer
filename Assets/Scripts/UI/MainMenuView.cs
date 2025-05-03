using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuView : BaseView
{
    [SerializeField] private LobbySelectionView lobbySelectionView;
    [SerializeField] private NameView nameView;

    private void Start()
    {
        InitUnityServices.Instance.Init();
        if (PlayerPrefsHelper.Instance.GetInt(Constant.IS_NAME_VIEW_SHOWN, 0) == 0)
            nameView.ShowView();
    }

    public void OnPlayButtonClick()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
            InitUnityServices.Instance.Init();
        lobbySelectionView.ShowView();
        HideView();
    }
}
