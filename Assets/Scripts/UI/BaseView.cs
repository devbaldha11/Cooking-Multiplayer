using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseView : MonoBehaviour
{
    public virtual void ShowView(Action action = null)
    {
        action?.Invoke();
        gameObject.SetActive(true);
    }

    public virtual void HideView(Action action = null)
    {
        action?.Invoke();
        gameObject.SetActive(false);
    }
}
