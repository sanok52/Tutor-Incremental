using System;
using System.Collections.Generic;
using UnityEngine;

// –ецепт (теперь ScriptableObject, чтобы можно было хранить как под-ассет)
[Serializable]
public class Recipe<E, TIngredientInstance>
    where E : Enum
    where TIngredientInstance : IngredientInstance<E>
{
    public E InputA;
    public E InputB;
    public E SimpleResult;
    public List<MergeBehaviour<E, TIngredientInstance>> Behaviours = new();
    public List<SpecialResultGroup<E>> SpecialResults = new();

    public E GetResult(TIngredientInstance inputA, TIngredientInstance inputB,
    out TIngredientInstance resultInstance)
    {
        resultInstance = null;
        E resultType = SimpleResult;
        bool breakChain = false;

        for (int i = 0; i < Behaviours.Count; i++)
        {
            if (breakChain) break;
            var behaviour = Behaviours[i];
            if (behaviour == null) continue;

            E[] special = (i < SpecialResults.Count) ? SpecialResults[i].Results.ToArray() : null;
            resultType = behaviour.GetResult(this, inputA, inputB, special,
                out var tempResult, out breakChain);
            if (tempResult != null) resultInstance = tempResult;
        }

        if (EqualityComparer<E>.Default.Equals(resultType, default(E)))
            resultType = SimpleResult;

        // ≈сли поведени€ не дали готовый экземпл€р Ц создаЄм через фабрику
        if (resultInstance == null)
        {
            resultInstance = (TIngredientInstance)inputA.CreateResult(resultType, inputA, inputB);
        }

        return resultType;
    }
}

[Serializable]
public class SpecialResultGroup<E> where E : Enum
{
    public List<E> Results = new();
}