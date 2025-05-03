using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLeftView : BaseView
{
    [SerializeField] private Image fillImage;

    private void Awake()
    {
        fillImage.fillAmount = 0;
    }

    public void SetFillAmount(float fillAmount)
    {
        fillImage.fillAmount = fillAmount;
    }
}
