using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartCountdownView : BaseView
{
    public TMP_Text countdownText;

    public void SetCountdownText(string text)
    {
        countdownText.text = text;
    }
}
