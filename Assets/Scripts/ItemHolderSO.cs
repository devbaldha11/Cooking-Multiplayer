using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CookingGame/SO/IngredientSO")]
public class ItemHolderSO : ScriptableObject
{
    [SerializeField] private List<Item> itemsList;
    [SerializeField] private PlateItem plateItem;

    public PlateItem GetPlateItem()
    {
        return plateItem;
    }

    public Item GetItemPrefabByItemID(ItemID item)
    {
        for (int i = 0; i < itemsList.Count; i++)
        {
            if (itemsList[i].ItemId == item)
                return itemsList[i];
        }
        return null;
    }

    public Sprite GetItemSpriteByItemID(ItemID item)
    {
        for (int i = 0; i < itemsList.Count; i++)
        {
            if (itemsList[i].ItemId == item)
                return itemsList[i].ItemImage;
        }
        return null;
    }

    public string GetItemNameByItemID(ItemID item)
    {
        for (int i = 0; i < itemsList.Count; i++)
        {
            if (itemsList[i].ItemId == item)
                return itemsList[i].ItemName;
        }
        return null;
    }
}
