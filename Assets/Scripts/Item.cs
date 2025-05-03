using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour
{
    public FollowTransform followTransform;

    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemImage;
    [SerializeField] private ItemID itemID;

    public Sprite ItemImage { get => itemImage; }
    public string ItemName { get => itemName; }
    public ItemID ItemId { get => itemID; }

    private void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }
}

public enum ItemID
{
    none = -1,
    bread = 0,
    cabbage = 1,
    cabbageSlice = 2,
    cheeseBlock = 3,
    cheeseSlice = 4,
    meatPattyBurned = 5,
    meatPattyCooked = 6,
    meatPattyUncooked = 7,
    plate = 8,
    tomato = 9,
    tomatoSlice = 10,
}
