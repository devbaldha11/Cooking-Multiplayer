using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameView : BaseView
{
    public TMP_Text placeholderText;
    public TMP_InputField nameInputField;

    private void Start()
    {
        placeholderText.text = "Guest" + Random.Range(0, 999);
    }

    public void OnConfirmButtonClick()
    {
        if (string.IsNullOrEmpty(nameInputField.text))
            PlayerPrefsHelper.Instance.SetString(Constant.PLAYER_NAME, placeholderText.text);
        else
            PlayerPrefsHelper.Instance.SetString(Constant.PLAYER_NAME, nameInputField.text);
        PlayerPrefsHelper.Instance.SetInt(Constant.IS_NAME_VIEW_SHOWN, 1);
        HideView();
    }
}
