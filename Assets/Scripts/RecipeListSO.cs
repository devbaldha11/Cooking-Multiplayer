using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CookingGame/SO/RecipeListSO")]
public class RecipeListSO : ScriptableObject
{
    [SerializeField] private List<RecipeSO> recipeSOList;

    public int NumberOfRecipes
    {
        get
        {
            return recipeSOList.Count;
        }
    }

    public RecipeSO GetRandomRecipe()
    {
        return recipeSOList[Random.Range(0, recipeSOList.Count)];
    }

    public RecipeSO GetRecipe(int index)
    {
        return recipeSOList[index];
    }
}
