using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateCounter : BaseCounter
{
    private PlateItem plate;

    private void Awake()
    {
        plate = myItem as PlateItem;
    }

    public override void Interact()
    {
        base.Interact();

        if (Player.LocalInstance.HasPlate())
            return;

        if (Player.LocalInstance.HasItem && plate.CanAddItemInPlate(Player.LocalInstance.MyItem) || !Player.LocalInstance.HasItem)
        {
            ShowGetPlatePopup();
            onInteractButtonClickAction = GivePlayerPlate;
        }
    }

    public override void OnInteractButtonClick()
    {
        onInteractButtonClickAction?.Invoke();
    }

    private void ShowGetPlatePopup()
    {
        itemUICanvas.SetAndEnableCanvas(Constant.PICK_UP_PLATE);
    }

    private void GivePlayerPlate()
    {
        SpawnPlate(Player.LocalInstance.OwnerClientId);
    }

    private void OnPlateSpawn(ulong playerID, Item item)
    {
        SetMyItem(item);
        Player player = PlayerManager.GetPlayer(playerID);
        if (player.HasItem)
        {
            PlateItem plate = myItem as PlateItem;
            plate.AddItemInPlate(player.MyItem);
            player.RemoveMyItem(true);
            myItem = plate;
        }
        player.SetMyItem(this, player.OwnerClientId, false);
    }

    private void SpawnPlate(ulong playerID)
    {
        SpawnPlateServerRpc(this.NetworkObject, playerID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlateServerRpc(NetworkObjectReference counterNetworkObjectReference, ulong playerID)
    {
        PlateItem item = Instantiate(ResourceManager.Instance.itemSO.GetPlateItem());
        NetworkObject networkObject = item.NetworkObject;
        networkObject.Spawn(true);

        counterNetworkObjectReference.TryGet(out NetworkObject counterNetworkObject);
        counterNetworkObject.TryGetComponent(out PlateCounter counter);
        if (counter != null)
        {
            counter?.OnPlateSpawn(playerID, item);
        }
    }
}
