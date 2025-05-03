using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class OrderManager : NetworkManager<OrderManager>
{
    [SerializeField] private OrderView orderUI;
    [SerializeField] private RecipeListSO recipesListSO;
    [SerializeField] private List<RecipeSO> waitingOrderList;
    [SerializeField] private int minOrderWaitTime = 2;
    [SerializeField] private int maxOrderWaitTime = 8;
    [SerializeField] private int maxRecipe;

    private NetworkVariable<int> recipeDeliveredNetworkVariable = new NetworkVariable<int>();
    private NetworkVariable<int> itemToRemove = new NetworkVariable<int>();
    private Coroutine coroutine;

    public int RecipeDelivered => recipeDeliveredNetworkVariable.Value;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public bool CanDeliveryItem(PlateItem plate)
    {
        for (int i = 0; i < waitingOrderList.Count; i++)
        {
            if (waitingOrderList[i].ingredientsList.Count != plate.ItemsInPlate.Count)
                continue;

            int matchIngredient = 0;
            for (int j = 0; j < waitingOrderList[i].ingredientsList.Count; j++)
            {
                if (!plate.ItemsInPlate.Contains(waitingOrderList[i].ingredientsList[j]))
                    break;
                matchIngredient++;
            }
            if (matchIngredient == waitingOrderList[i].ingredientsList.Count)
            {
                SetItemToRemoveServerRpc(waitingOrderList[i].recipeId);
                return true;
            }
        }
        return false;
    }

    public void DeliverOrder()
    {
        DeliverOrderServerRpc();
    }

    public void StartGeneratingOrder()
    {
        GenerateOrderServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetItemToRemoveServerRpc(int id)
    {
        itemToRemove.Value = id;
    }

    [ServerRpc]
    private void GenerateOrderServerRpc()
    {
        coroutine = StartCoroutine(GenerateOrderCO(true));
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverOrderServerRpc()
    {
        recipeDeliveredNetworkVariable.Value++;
        DeliverOrderClientRpc();
        if (coroutine == null)
            coroutine = StartCoroutine(GenerateOrderCO(false));
    }

    [ClientRpc]
    private void DeliverOrderClientRpc()
    {
        for (int i = 0; i < waitingOrderList.Count; i++)
        {
            if (waitingOrderList[i].recipeId == itemToRemove.Value)
            {
                waitingOrderList.Remove(waitingOrderList[i]);
                orderUI.RemoveOrderSlot(itemToRemove.Value);
                return;
            }
        }
    }

    private IEnumerator GenerateOrderCO(bool isFirstOrder)
    {
        while (waitingOrderList.Count < maxRecipe)
        {
            int randomWait = isFirstOrder ? 0 : Random.Range(minOrderWaitTime, maxOrderWaitTime);

            yield return new WaitForSeconds(randomWait);

            SyncOrderClientRpc(Random.Range(0, recipesListSO.NumberOfRecipes));
            isFirstOrder = false;
        }
        OnGenerateOrderComplete();
    }

    [ClientRpc]
    private void SyncOrderClientRpc(int index)
    {
        RecipeSO recipeSO = recipesListSO.GetRecipe(index);
        waitingOrderList.Add(recipeSO);
        orderUI.AddOrderSlot(recipeSO);
    }

    private void OnGenerateOrderComplete()
    {
        coroutine = null;
    }
}
