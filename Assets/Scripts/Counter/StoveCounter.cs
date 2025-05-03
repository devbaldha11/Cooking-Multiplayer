using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StoveCounter : BaseCounter
{
    [SerializeField] private Image fillBar;
    [SerializeField] private GameObject stoveOnVisual;
    [SerializeField] private GameObject fryingParticle;
    [SerializeField] private float fryingTime;
    [SerializeField] private float overCookedTime;
    [SerializeField] private List<ItemID> validItemList = new List<ItemID>();
    [SerializeField] private List<ItemClass> fryingItemList = new List<ItemClass>();
    private ItemID currentItemID;
    private CookingState currentState;

    [System.Serializable]
    public class ItemClass
    {
        public ItemID itemID;
        public ItemID cookedItemID;
        public ItemID burnedItemID;
    }

    public override void Interact()
    {
        base.Interact();
        if (Player.LocalInstance.HasItem && !HasItem)
        {
            if (CanFryItem(Player.LocalInstance.MyItem))
            {
                onInteractButtonClickAction = StartFryingItem;
                ShowFryingPopup();
            }
        }

        if (currentState == CookingState.cooked)
        {
            if (Player.LocalInstance.HasPlate() && HasItem)
            {
                PlateItem item = Player.LocalInstance.MyItem as PlateItem;
                if (item.CanAddItemInPlate(myItem))
                {
                    ShowPickUpItemPopup();
                    onInteractButtonClickAction = GiveItemInPlateOfPlayer;
                }
            }
        }

        if (currentState == CookingState.cooked || currentState == CookingState.burned)
        {
            if (!Player.LocalInstance.HasItem && HasItem)
            {
                ShowPickUpItemPopup();
                onInteractButtonClickAction = GiveItemToPlayer;
            }
        }
    }

    public override void OnInteractButtonClick()
    {
        onInteractButtonClickAction?.Invoke();
    }

    private bool CanFryItem(Item item)
    {
        if (validItemList.Contains(item.ItemId))
            return true;
        return false;
    }

    private void ShowFryingPopup()
    {
        itemUICanvas.SetAndEnableCanvas(Constant.FRY_ITEM);
    }

    private void ShowPickUpItemPopup()
    {
        itemUICanvas.SetAndEnableCanvas(Constant.PICK_UP_ITEM);
    }

    private void GiveItemInPlateOfPlayer()
    {
        stoveOnVisual.SetActive(false);
        fryingParticle.gameObject.SetActive(false);
        fillBar.transform.parent.gameObject.SetActive(false);
        StopAllCoroutines();
        PlateItem item = Player.LocalInstance.MyItem as PlateItem;
        item.AddItemInPlate(myItem);
        RemoveMyItem(true);
        OnInteractionDone();
    }

    private void GiveItemToPlayer()
    {
        StopStoveServerRpc();
        Player.LocalInstance.SetMyItem(this, Player.LocalInstance.OwnerClientId, true);
    }

    private void StartFryingItem()
    {
        currentItemID = Player.LocalInstance.MyItem.ItemId;
        SetMyItem(Player.LocalInstance.MyItem, Player.LocalInstance.OwnerClientId);
        StartFryingCOServerRpc();
        itemUICanvas.DisableCanvas();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartFryingCOServerRpc()
    {
        FryingCOClientRpc();
    }

    [ClientRpc]
    private void FryingCOClientRpc()
    {
        StartCoroutine(FryingCO());
    }

    [ServerRpc(RequireOwnership = false)]
    private void StopStoveServerRpc()
    {
        StopStoveClientRpc();
    }

    [ClientRpc]
    private void StopStoveClientRpc()
    {
        stoveOnVisual.SetActive(false);
        fryingParticle.gameObject.SetActive(false);
        fillBar.transform.parent.gameObject.SetActive(false);
        StopAllCoroutines();
    }

    private IEnumerator FryingCO()
    {
        currentState = CookingState.frying;
        float time = 0;
        fryingParticle.SetActive(true);
        stoveOnVisual.SetActive(true);
        fillBar.transform.parent.gameObject.SetActive(true);
        fillBar.color = Color.yellow;
        while (time < fryingTime)
        {
            time += Time.deltaTime;
            fillBar.fillAmount = (float)time / fryingTime;
            yield return null;
        }
        OnItemCooked();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartBurnedItemCOServerRpc()
    {
        BurnedItemCOClientRpc();
    }

    [ClientRpc]
    private void BurnedItemCOClientRpc()
    {
        StartCoroutine(BurnedItemCO());
    }

    private IEnumerator BurnedItemCO()
    {
        InitCookedItem();
        float time = 0;
        fillBar.color = Color.red;
        while (time < overCookedTime)
        {
            time += Time.deltaTime;
            fillBar.fillAmount = (float)time / overCookedTime;
            yield return null;
        }
        yield return null;
        OnItemBurned();
    }

    private void OnItemCooked()
    {
        currentState = CookingState.cooked;
        InitCookedItem();
        StartBurnedItemCOServerRpc();
        OnInteractionDone();
    }

    private void OnItemBurned()
    {
        currentState = CookingState.burned;
        stoveOnVisual.SetActive(false);
        fryingParticle.gameObject.SetActive(false);
        fillBar.transform.parent.gameObject.SetActive(false);
        InitBurnedItem();
    }

    private void InitCookedItem()
    {
        foreach (var item in fryingItemList)
        {
            if (item.itemID == currentItemID)
            {
                RemoveMyItem(true);
                MultiplayerManager.Instance.SpawnItem(item.cookedItemID, this.NetworkObject);
            }
        }
    }

    private void InitBurnedItem()
    {
        foreach (var item in fryingItemList)
        {
            if (item.itemID == currentItemID)
            {
                RemoveMyItem(true);
                MultiplayerManager.Instance.SpawnItem(item.burnedItemID, this.NetworkObject);
            }
        }
    }
}

public enum CookingState
{
    idle,
    frying,
    cooked,
    burned
}