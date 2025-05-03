using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public override void Interact()
    {
        base.Interact();
        if (Player.LocalInstance.HasPlate())
        {
            PlateItem item = Player.LocalInstance.MyItem as PlateItem;
            if (OrderManager.Instance.CanDeliveryItem(item))
                ShowDeliverOrder();
        }
    }

    public override void OnInteractButtonClick()
    {
        Player.LocalInstance.RemoveMyItem(true);
        OrderManager.Instance.DeliverOrder();
        Interact();
    }

    private void ShowDeliverOrder()
    {
        itemUICanvas.SetAndEnableCanvas(Constant.DELIVER_ORDER);
    }
}
