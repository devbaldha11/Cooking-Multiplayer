using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCounter : BaseCounter
{
    public override void Interact()
    {
        base.Interact();
        ShowTrashPopup();
    }

    public override void OnInteractButtonClick()
    {
        onInteractButtonClickAction?.Invoke();
    }

    private void ShowTrashPopup()
    {
        if (Player.LocalInstance.HasItem)
        {
            itemUICanvas.SetAndEnableCanvas(Constant.TRASH_ITEM);
            onInteractButtonClickAction = TrashPlayerItem;
        }
    }

    private void TrashPlayerItem()
    {
        Player.LocalInstance.RemoveMyItem(true);
        itemUICanvas.DisableCanvas();
    }
}
