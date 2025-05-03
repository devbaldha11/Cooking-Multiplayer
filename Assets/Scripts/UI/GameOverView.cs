using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverView : BaseView
{
    [SerializeField] private TMP_Text deliveredText;

    public void SetData(int deliveredRecipe)
    {
        deliveredText.text = "Delivered Recipes: " + deliveredRecipe;
        ShowView();
    }

    public void OnCloseButtonClick()
    {
        HideView();
    }
}
