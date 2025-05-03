using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateItem : Item
{
    [SerializeField] private List<ItemID> validItemList = new List<ItemID>();
    [SerializeField] private List<ItemID> itemsInPlate = new List<ItemID>();
    [SerializeField] public List<ItemImageClass> itemImageList;

    [System.Serializable]
    public struct ItemImageClass
    {
        public ItemID itemID;
        public GameObject itemGameObject;
        public GameObject itemUI;
    }

    public List<ItemID> ItemsInPlate => itemsInPlate;

    private void Awake()
    {
        for (int i = 0; i < itemImageList.Count; i++)
        {
            itemImageList[i].itemGameObject.SetActive(false);
        }
    }

    public bool CanAddItemInPlate(Item item)
    {
        if (validItemList.Contains(item.ItemId) && !itemsInPlate.Contains(item.ItemId))
            return true;
        return false;
    }

    public void AddItemInPlate(Item item)
    {
        AddItemInPlateServerRpc((int)item.ItemId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddItemInPlateServerRpc(int itemId)
    {
        AddItemInPlateClientRpc(itemId);
    }
     
    [ClientRpc]
    private void AddItemInPlateClientRpc(int itemId)
    {
        ItemID item = (ItemID)itemId;
        itemsInPlate.Add(item);

        for (int i = 0; i < itemImageList.Count; i++)
        {
            if (itemImageList[i].itemID == item)
            {
                itemImageList[i].itemGameObject.SetActive(true);
                itemImageList[i].itemUI.SetActive(true);
            }
        }
    }
}
