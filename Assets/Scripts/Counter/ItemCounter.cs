using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCounter : BaseCounter
{
    public override void Interact()
    {
        base.Interact();
        if (!Player.LocalInstance.HasItem)
        {
            ShowGiveItemToPlayerPopup();
            onInteractButtonClickAction = GiveItemToPlayer;
        }

        if (Player.LocalInstance.HasPlate())
        {
            PlateItem item = Player.LocalInstance.MyItem as PlateItem;
            if (item.CanAddItemInPlate(myItem))
            {
                ShowGiveItemToPlayerPopup();
                onInteractButtonClickAction = GiveItemInPlateOfPlayer;
            }
        }
    }

    public override void OnInteractButtonClick()
    {
        onInteractButtonClickAction?.Invoke();
    }

    private void ShowGiveItemToPlayerPopup()
    {
        itemUICanvas.SetAndEnableCanvas(Constant.PICK_UP_ITEM);
    }

    private void GiveItemToPlayer()
    {
        MultiplayerManager.Instance.SpawnItem(myItem.ItemId, NetworkObject, Player.LocalInstance.PlayerID);
        //OnInteractionDone();
    }

    private void GiveItemInPlateOfPlayer()
    {
        PlateItem item = Player.LocalInstance.MyItem as PlateItem;
        item.AddItemInPlate(myItem);
        OnInteractionDone();
    }
}
