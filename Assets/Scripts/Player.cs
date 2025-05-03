using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float playerHeight;
    [SerializeField] private float playerRadius;
    [SerializeField] private float maxRaycastDistance;
    [SerializeField] private BaseCounter selectedCounter;
    [SerializeField] private Item myItem;
    [SerializeField] private TMP_Text playerNameText;

    public Transform itemHoldPoint;
    private bool isPlayerMoving;
    private bool canMove;
    private Vector3 movementDirection;
    private Vector3 lastMovementDirection;

    public bool HasItem { get => myItem != null; }

    public bool IsPlayerMoving { get => isPlayerMoving; }

    public ulong PlayerID { get => OwnerClientId; }

    public BaseCounter SelectedCounter { get => selectedCounter; set => selectedCounter = value; }

    public Item MyItem { get => myItem; set => myItem = value; }

    public bool HasPlate()
    {
        if (myItem != null)
            return myItem.ItemId == ItemID.plate;
        return false;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        if (!LocalInstance)
            LocalInstance = this;

        MultiplayerManager.Instance.SetLookAtCamera(transform);
    }

    public override void OnNetworkDespawn()
    {
        if (HasItem)
            RemoveMyItem(true);

        if (LocalInstance)
            Destroy(gameObject);
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        HandleMovement();
        HandleInteraction();
    }

    public override void OnDestroy()
    {
        if (LocalInstance)
            Destroy(gameObject);
    }

    private void HandleMovement()
    {
        if (!InputManager.Instance)
            return;

        Vector2 inputVector2 = InputManager.Instance.GetPlayerMovementVectorNormalized();
        movementDirection = new Vector3(inputVector2.x, 0, inputVector2.y);

        canMove = CapsuleCast();

        if (!canMove)
        {
            movementDirection = new Vector3(inputVector2.x, 0, 0);
            canMove = CapsuleCast();
            if (canMove)
                MovePlayer();
            if (!canMove)
            {
                movementDirection = new Vector3(0, 0, inputVector2.y);
                canMove = CapsuleCast();
                if (canMove)
                    MovePlayer();
            }
        }

        if (canMove)
            MovePlayer();

        isPlayerMoving = movementDirection != Vector3.zero;

        transform.forward = Vector3.Slerp(transform.forward, movementDirection, turnSpeed * Time.deltaTime);
    }

    public static Player GetPlayerByClientId(ulong clientId)
    {
        return PlayerManager.GetPlayer(clientId);
    }

    public void SetPlayerNameText(string name)
    {
        playerNameText.text = name;
    }

    public void SetMyItem(BaseCounter baseCounter, ulong playerID, bool removeCounterItem, bool destroyCounterItem = false)
    {
        SetMyItemServerRpc(baseCounter.NetworkObject, playerID, removeCounterItem, destroyCounterItem);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetMyItemServerRpc(NetworkObjectReference itemNetworkObjectReference, ulong playerID, bool removeCounterItem, bool destroyCounterItem = false)
    {
        SetMyItemClientRpc(itemNetworkObjectReference, playerID, removeCounterItem, destroyCounterItem);
    }

    [ClientRpc]
    public void SetMyItemClientRpc(NetworkObjectReference itemNetworkObjectReference, ulong playerID, bool removeCounterItem, bool destroyCounterItem = false)
    {
        itemNetworkObjectReference.TryGet(out NetworkObject itemNetworkObject);
        itemNetworkObject.TryGetComponent(out BaseCounter baseCounter);
        PlayerManager.GetPlayer(playerID).myItem = baseCounter.MyItem;
        PlayerManager.GetPlayer(playerID).myItem.followTransform.SetFollowTransform(PlayerManager.GetPlayer(playerID).itemHoldPoint);
        if (destroyCounterItem)
            baseCounter.MyItem.NetworkObject.Despawn(true);
        if (removeCounterItem)
            baseCounter.MyItem = null;
        baseCounter.OnInteractionDone(playerID);
    }

    public void RemoveMyItem(bool destroy = false)
    {
        if (destroy)
            MultiplayerManager.Instance.DespawnItem(myItem.NetworkObject, PlayerID);
        else
            myItem = null;
    }

    private void MovePlayer()
    {
        transform.position += movementDirection * movementSpeed * Time.deltaTime;
    }

    private bool CapsuleCast()
    {
        return !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, movementDirection, maxRaycastDistance);
    }

    private void HandleInteraction()
    {
        if (!InputManager.Instance)
            return;

        Vector2 inputVector2 = InputManager.Instance.GetPlayerMovementVectorNormalized();
        movementDirection = new Vector3(inputVector2.x, 0, inputVector2.y);

        if (movementDirection != Vector3.zero)
            lastMovementDirection = movementDirection;

        Physics.Raycast(transform.position, lastMovementDirection, out RaycastHit raycastHit, 2f);

        if (raycastHit.collider == null)
        {
            if (selectedCounter != null)
            {
                selectedCounter.OnCounterDeselect();
                selectedCounter = null;
            }
            return;
        }

        raycastHit.transform.TryGetComponent(out BaseCounter counter);
        if (counter != null && selectedCounter != counter)
        {
            selectedCounter = counter;
            counter.Interact();
        }
    }
}
