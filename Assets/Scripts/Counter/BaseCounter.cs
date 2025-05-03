using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour
{
    [SerializeField] private Material counterHighlight;
    [SerializeField] private Material counterNormal;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] protected Transform counterTop;
    [SerializeField] protected Item myItem;
    [SerializeField] public UICanvas itemUICanvas;
    [SerializeField] protected Action onInteractButtonClickAction;
    [SerializeField] protected Action onAlternateIntractButtonClickAction;

    public Item MyItem { get => myItem; set => myItem = value; }

    public BaseCounter Counter { get => this; }

    public bool HasItem { get => myItem != null; }

    private void Update()
    {
        if (Player.LocalInstance != null && Player.LocalInstance.SelectedCounter != this)
            OnCounterDeselect();
    }

    public virtual void Interact()
    {
        meshRenderer.material = counterHighlight;
        itemUICanvas.DisableCanvas();
    }

    public virtual void OnCounterDeselect()
    {
        meshRenderer.material = counterNormal;
        itemUICanvas.DisableCanvas();
    }

    public virtual void OnInteractButtonClick()
    {

    }

    public virtual void OnAlternateInteractButtonClick()
    {

    }

    public virtual void SetMyItem(Item item, ulong playerid = ulong.MaxValue)
    {
        SetMyItemServerRpc(item.NetworkObject, playerid);
    }

    public virtual void OnItemSpawn(ulong playerID, Item item)
    {
        SetMyItem(item);
        if (playerID != ulong.MaxValue)
            Player.LocalInstance.SetMyItem(this, playerID, false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMyItemServerRpc(NetworkObjectReference networkObjectReference, ulong playerid)
    {
        SetMyItemClientRpc(networkObjectReference, playerid);
    }

    [ClientRpc]
    private void SetMyItemClientRpc(NetworkObjectReference networkObjectReference, ulong playerid)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        myItem = networkObject.GetComponent<Item>();
        myItem.followTransform.SetFollowTransform(counterTop);
        if (playerid != ulong.MaxValue)
            PlayerManager.GetPlayer(playerid).RemoveMyItem();
    }

    protected void RemoveMyItem(bool destroy = false)
    {
        RemoveMyItemServerRpc(destroy, this.NetworkObject);
    }

    public virtual void OnAnoterInteractionDone()
    {
        onAlternateIntractButtonClickAction = null;
        Player.LocalInstance.SelectedCounter = null;
    }

    public virtual void OnInteractionDone(ulong playerId = ulong.MaxValue)
    {
        onInteractButtonClickAction = null;
        if (playerId != ulong.MaxValue)
            RemoveSelectedCounterServerRpc(playerId);
        else
            Player.LocalInstance.SelectedCounter = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveSelectedCounterServerRpc(ulong playerID)
    {
        RemoveSelectedCounterClientRpc(playerID);
    }

    [ClientRpc]
    private void RemoveSelectedCounterClientRpc(ulong playerID)
    {
        PlayerManager.GetPlayer(playerID).SelectedCounter = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveMyItemServerRpc(bool destroy, NetworkObjectReference networkObjectReference)
    {
        if (destroy)
        {
            networkObjectReference.TryGet(out NetworkObject networkObject);
            networkObject.TryGetComponent(out BaseCounter counter);
            if (counter != null)
                counter.myItem.NetworkObject.Despawn(true);
        }
        RemoveMyItemItemClientRpc(networkObjectReference);
    }

    [ClientRpc]
    private void RemoveMyItemItemClientRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);
        networkObject.TryGetComponent(out BaseCounter counter);
        if (counter != null)
            counter.myItem = null;
    }
}
