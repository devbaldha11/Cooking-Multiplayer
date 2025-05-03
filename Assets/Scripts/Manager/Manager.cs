using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Manager<T> : MonoBehaviour where T : Manager<T>
{
    public static T Instance;
    private ManagerInstanceLoader managerInstanceLoader;

    public bool IsLoaded
    {
        get
        {
            if (managerInstanceLoader == null)
                return true;
            return managerInstanceLoader.isLoaded;
        }
    }

    public virtual void Awake()
    {
        managerInstanceLoader = GetComponent<ManagerInstanceLoader>();
        OnLoadingStart();
        if (!Instance)
            Instance = this as T;
    }

    public virtual void OnDestroy()
    {
        if (Instance)
            Destroy(gameObject);
    }

    public void OnLoadingStart()
    {
        if (managerInstanceLoader != null)
            managerInstanceLoader.OnLoadingStart();
    }

    public void OnLoadingDone()
    {
        if (managerInstanceLoader != null)
            managerInstanceLoader.OnLoadingDone();
    }
}

public class NetworkManager<T> : NetworkBehaviour where T : NetworkManager<T>
{
    public static T Instance;
    private ManagerInstanceLoader managerInstanceLoader;

    public bool IsLoaded
    {
        get
        {
            if (managerInstanceLoader == null)
                return true;
            return managerInstanceLoader.isLoaded;
        }
    }

    public virtual void Awake()
    {
        managerInstanceLoader = GetComponent<ManagerInstanceLoader>();
        OnLoadingStart();
        if (!Instance)
            Instance = this as T;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    public override void OnDestroy()
    {
        if (Instance)
            Destroy(gameObject);
    }

    public void OnLoadingStart()
    {
        if (managerInstanceLoader != null)
            managerInstanceLoader.OnLoadingStart();
    }

    public void OnLoadingDone()
    {
        if (managerInstanceLoader != null)
            managerInstanceLoader.OnLoadingDone();
    }
}

public class GameplayUIManager<T> : MonoBehaviour where T : GameplayUIManager<T>
{
    public List<BaseView> viewsList;

    public void ShowView(BaseView baseView)
    {
        if (viewsList.Contains(baseView))
        {
            baseView.ShowView();
        }
    }


}

public enum LoadingType
{
    auto,
    manual
}