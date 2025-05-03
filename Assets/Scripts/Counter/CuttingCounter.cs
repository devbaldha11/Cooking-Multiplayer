using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private CuttableItemIdSO cuttableItemIdSO;

    private Coroutine coroutine;

    public override void Interact()
    {
        base.Interact();
        if (Player.LocalInstance.HasItem && !HasItem)
        {
            if (cuttableItemIdSO.CanCutItem(Player.LocalInstance.MyItem.ItemId))
            {
                onInteractButtonClickAction = CutItem;
                ShowCutItemPopup();
            }
        }

        if (Player.LocalInstance.HasPlate() && HasItem)
        {
            PlateItem item = Player.LocalInstance.MyItem as PlateItem;
            if (item.CanAddItemInPlate(myItem))
            {
                ShowGiveItemToPlayerPopup();
                onInteractButtonClickAction = GiveItemInPlateOfPlayer;
            }
        }

        if (!Player.LocalInstance.HasItem && HasItem)
        {
            ShowGiveItemToPlayerPopup();
            onInteractButtonClickAction = GiveItemToPlayer;
        }
    }

    public override void OnInteractButtonClick()
    {
        itemUICanvas.DisableCanvas();
        onInteractButtonClickAction?.Invoke();
    }

    private void ShowCutItemPopup()
    {
        itemUICanvas.SetAndEnableCanvas(Constant.CUT_ITEM);
    }

    private void CutItem()
    {
        if (coroutine != null)
            coroutine = null;
        coroutine = StartCoroutine(CutItemCO());
    }

    private IEnumerator CutItemCO()
    {
        WaitForSeconds wait = new WaitForSeconds(3f);
        SetMyItem(Player.LocalInstance.MyItem, Player.LocalInstance.OwnerClientId);
        yield return wait;
        SetCuttedItem();
    }

    private void SetCuttedItem()
    {
        ItemID id = cuttableItemIdSO.GetCuttedItemID(myItem.ItemId);
        RemoveMyItem(true);
        MultiplayerManager.Instance.SpawnItem(id, base.NetworkObject);
        OnInteractionDone();
    }

    private void ShowGiveItemToPlayerPopup()
    {
        itemUICanvas.SetAndEnableCanvas(Constant.PICK_UP_ITEM);
    }

    private void GiveItemInPlateOfPlayer()
    {
        PlateItem item = Player.LocalInstance.MyItem as PlateItem;
        item.AddItemInPlate(myItem);
        RemoveMyItem(true);
        OnInteractionDone();
    }

    private void GiveItemToPlayer()
    {
        Player.LocalInstance.SetMyItem(this, Player.LocalInstance.OwnerClientId, true);
        //RemoveMyItem();
        //OnInteractionDone();
    }
}
