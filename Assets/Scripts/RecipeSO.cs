using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CookingGame/SO/RecipeSO")]
public class RecipeSO : ScriptableObject
{
    public string name;
    public int recipeId;
    public List<ItemID> ingredientsList;
}
