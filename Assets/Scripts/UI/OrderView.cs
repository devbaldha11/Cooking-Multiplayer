using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderView : MonoBehaviour
{
    [SerializeField] private List<OrderSlot> orderSlotsList;
    [SerializeField] private OrderSlot orderSlot;
    [SerializeField] private Transform parent;

    public void AddOrderSlot(RecipeSO recipe)
    {
        OrderSlot slot = Instantiate(orderSlot, parent);
        slot.InitSlot(recipe);
        orderSlotsList.Add(slot);
    }

    public void RemoveOrderSlot(int recipeId)
    {
        for (int i = 0; i < orderSlotsList.Count; i++)
        {
            if (orderSlotsList[i].recipeSO.recipeId == recipeId)
            {
                Destroy(orderSlotsList[i].gameObject);
                orderSlotsList.Remove(orderSlotsList[i]);
                return;
            }
        }
    }
}
