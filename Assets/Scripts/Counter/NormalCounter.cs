using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalCounter : BaseCounter
{
    public override void Interact()
    {
        base.Interact();

        Item playerItem = Player.LocalInstance.MyItem;
        bool playerHasItem = Player.LocalInstance.HasItem;
        bool playerHasPlate = Player.LocalInstance.HasPlate();

        if ((!HasItem && playerHasItem) || (HasItem && !playerHasItem))
        {
            ShowTransferItemPopup();
            return;
        }

        if (HasPlate())
        {
            PlateItem plate = myItem as PlateItem;
            if (plate.CanAddItemInPlate(playerItem))
            {
                ShowPickUpAndAddItemInPlatePopup();
                return;
            }
        }

        if (playerHasPlate)
        {
            PlateItem plate = playerItem as PlateItem;
            if (plate.CanAddItemInPlate(myItem))
            {
                ShowPickUpAndAddItemInPlatePopup();
                return;
            }
        }

        if ((HasItem && playerHasItem))
        {
            ShowSwitchItemPopup();
            return;
        }
    }

    public override void OnInteractButtonClick()
    {
        onInteractButtonClickAction?.Invoke();
    }

    public override void OnAlternateInteractButtonClick()
    {
        onAlternateIntractButtonClickAction?.Invoke();
    }

    public bool HasPlate()
    {
        if (myItem != null)
            return myItem.ItemId == ItemID.plate;
        return false;
    }

    private void ShowTransferItemPopup()
    {
        itemUICanvas.SetAndEnableCanvas(Constant.TRANSFER_ITEM);
        onInteractButtonClickAction = TransferItem;
    }

    private void ShowSwitchItemPopup()
    {
        itemUICanvas.SetAndEnableCanvas(Constant.SWITCH_ITEM);
        onInteractButtonClickAction = SwitchItems;
    }

    private void ShowPickUpAndAddItemInPlatePopup()
    {
        itemUICanvas.SetAndEnableCanvas(Constant.PICK_UP_ITEM, Constant.PUT_ITEM, true);
        onInteractButtonClickAction = GiveItemToPlayer;
        onAlternateIntractButtonClickAction = PutItemOnTable;
    }

    private void TransferItem()
    {
        Item item = Player.LocalInstance.MyItem;
        if (item != null)
        {
            SetMyItem(item, Player.LocalInstance.OwnerClientId);
            //Player.LocalInstance.RemoveMyItem();
            OnInteractionDone();
        }
        else
        {
            Player.LocalInstance.SetMyItem(this, Player.LocalInstance.OwnerClientId, true);
            //RemoveMyItem();
        }
    }

    private void SwitchItems()
    {
        Item item = Player.LocalInstance.MyItem;
        Player.LocalInstance.SetMyItem(this, Player.LocalInstance.OwnerClientId, false);
        SetMyItem(item);
        OnInteractionDone();
    }

    private void GiveItemToPlayer()
    {
        if (HasPlate())
        {
            PlateItem item = myItem as PlateItem;
            item.AddItemInPlate(Player.LocalInstance.MyItem);
            Player.LocalInstance.RemoveMyItem(true);
            Player.LocalInstance.SetMyItem(this, Player.LocalInstance.OwnerClientId, true);
            //RemoveMyItem();
        }
        else
        {
            PlateItem item = Player.LocalInstance.MyItem as PlateItem;
            item.AddItemInPlate(myItem);
            RemoveMyItem(true);
            OnInteractionDone();
        }
    }

    private void PutItemOnTable()
    {
        if (HasPlate())
        {
            PlateItem item = myItem as PlateItem;
            item.AddItemInPlate(Player.LocalInstance.MyItem);
            Player.LocalInstance.RemoveMyItem(true);
        }
        else
        {
            PlateItem item = Player.LocalInstance.MyItem as PlateItem;
            item.AddItemInPlate(myItem);
            RemoveMyItem(true);
            SetMyItem(item, Player.LocalInstance.OwnerClientId);
            //Player.LocalInstance.RemoveMyItem();
        }
        OnAnoterInteractionDone();
    }
}
