using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderSlot : MonoBehaviour
{
    public RecipeSO recipeSO;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private OrderImage itemImage;
    [SerializeField] private Transform parent;

    public void InitSlot(RecipeSO recipe)
    {
        List<ItemID> ingredients = recipe.ingredientsList;
        recipeSO = recipe;
        titleText.text = recipe.name;
        for (int i = 0; i < ingredients.Count; i++)
        {
            OrderImage orderImage = Instantiate(itemImage, parent);
            orderImage.image.sprite = ResourceManager.Instance.itemSO.GetItemSpriteByItemID(ingredients[i]);
            orderImage.gameObject.SetActive(true);
        }
        gameObject.SetActive(true);
    }
}
