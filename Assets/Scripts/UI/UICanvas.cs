using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICanvas : MonoBehaviour
{
    [SerializeField] private BaseCounter myCounter;
    [SerializeField] private Button button;
    [SerializeField] private Button button1;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Text button1Text;
    [SerializeField] private GameObject ui;

    public void Awake()
    {
        ui.SetActive(false);
    }

    public void OnButtonClick()
    {
        myCounter.OnInteractButtonClick();
    }

    public void OnOtherButtonClick()
    {
        myCounter.OnAlternateInteractButtonClick();
    }

    public void SetAndEnableCanvas(string buttonText = null, string button1Text = null, bool showSeconButton = false)
    {
        button1.transform.parent.gameObject.SetActive(false);
        if (showSeconButton)
        {
            this.button1Text.text = button1Text;
            button1.transform.parent.gameObject.SetActive(true);
        }
        this.buttonText.text = buttonText;
        ui.SetActive(true);
    }

    public void DisableCanvas()
    {
        ui.SetActive(false);
    }
}
