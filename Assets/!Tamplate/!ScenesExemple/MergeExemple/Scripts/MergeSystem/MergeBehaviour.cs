using System;
using UnityEngine;

public class MergeBehaviour<E, TIngredientInstance> : MonoBehaviour
    where E : Enum
    where TIngredientInstance : IngredientInstance<E>
{
    public virtual E GetResult(
        Recipe<E, TIngredientInstance> recipe,
        TIngredientInstance inputA,
        TIngredientInstance inputB,
        E[] specialResults,
        out TIngredientInstance resultInstance,
        out bool breakChain)
    {
        resultInstance = null;
        breakChain = false;
        return default;
    }

    public virtual string[] GetSpecialResultTitles() => new[] { "Spec" };
}