using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MergeData<E, TIngredientData, TIngredientInstance, TRecipe> : ScriptableObject
    where E : Enum
    where TIngredientData : IngredientData<E>
    where TIngredientInstance : IngredientInstance<E>
    where TRecipe : Recipe<E, TIngredientInstance>
{
    public List<TIngredientData> AllIngredients = new();
    public List<TRecipe> AllRecipes = new();

    [HideInInspector] public List<Vector2> _editorIngredientPositions = new();
    [HideInInspector] public List<Vector2> _editorRecipePositions = new();

    private TRecipe FindRecipe(E inputA, E inputB)
    {
        return AllRecipes.FirstOrDefault(r =>
            (EqualityComparer<E>.Default.Equals(r.InputA, inputA) && EqualityComparer<E>.Default.Equals(r.InputB, inputB)) ||
            (EqualityComparer<E>.Default.Equals(r.InputA, inputB) && EqualityComparer<E>.Default.Equals(r.InputB, inputA)));
    }

    public E GetResult(TIngredientInstance inputA, TIngredientInstance inputB, out TIngredientInstance result, out TRecipe recipeUsed)
    {
        result = null;
        recipeUsed = FindRecipe(inputA.Type, inputB.Type);
        if (recipeUsed == null)
        {
            Debug.LogWarning($"Recipe for {inputA.Type} + {inputB.Type} not found");
            return default(E);
        }
        return recipeUsed.GetResult(inputA, inputB, out result);
    }
}