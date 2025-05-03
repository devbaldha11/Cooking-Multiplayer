using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    [SerializeField] private List<ManagerInstanceLoader> managers = new List<ManagerInstanceLoader>();

    public void Awake()
    {
        StartCoroutine(LoadManagers());
    }

    private IEnumerator LoadManagers()
    {
        foreach (var item in managers)
        {
            item.gameObject.SetActive(true);
            while (!item.isLoaded)
            {
                yield return 0;
            }
        }
    }
}
