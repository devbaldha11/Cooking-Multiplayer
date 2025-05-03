using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MessageView : BaseView
{
    public TMP_Text messageText;
    public GameObject closeButton;

    public void ShowView(string message, bool showCloseButton = true)
    {
        messageText.text = message;
        closeButton.SetActive(showCloseButton);
        ShowView();
    }

    public void OnCloseButtonClick()
    {
        HideView();
    }
}
