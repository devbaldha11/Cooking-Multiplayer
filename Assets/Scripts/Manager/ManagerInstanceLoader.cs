using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerInstanceLoader : MonoBehaviour
{
    public bool isLoaded;
    public LoadingType loadingType;

    private void Awake()
    {
        if (loadingType == LoadingType.auto)
            isLoaded = true;
    }

    public void OnLoadingStart()
    {
        if (loadingType == LoadingType.manual)
            isLoaded = false;
    }

    public void OnLoadingDone()
    {
        if (loadingType == LoadingType.auto)
            isLoaded = true;
    }
}
