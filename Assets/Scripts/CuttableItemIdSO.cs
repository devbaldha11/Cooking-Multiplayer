using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CookingGame/SO/CuttableItemIdSO")]
public class CuttableItemIdSO : ScriptableObject
{
    [SerializeField] private List<CuttableItemId> cuttableItemIdList;

    public bool CanCutItem(ItemID itemID)
    {
        for (int i = 0; i < cuttableItemIdList.Count; i++)
        {
            if (cuttableItemIdList[i].inputItemID == itemID)
                return true;
        }
        return false;
    }

    public ItemID GetCuttedItemID(ItemID itemID)
    {
        for (int i = 0; i < cuttableItemIdList.Count; i++)
        {
            if (cuttableItemIdList[i].inputItemID == itemID)
                return cuttableItemIdList[i].outputItemID;
        }
        return ItemID.none;
    }

    [System.Serializable]
    public struct CuttableItemId
    {
        public ItemID inputItemID;
        public ItemID outputItemID;
    }
}
