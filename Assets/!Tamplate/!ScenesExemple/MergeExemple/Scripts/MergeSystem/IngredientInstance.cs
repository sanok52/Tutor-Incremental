using System;

[Serializable]
public abstract class IngredientInstance<E> where E : Enum
{
    public E Type;
    public int Count;
    public int SelfCost;
    public abstract IngredientInstance<E> Clone();

    public virtual IngredientInstance<E> CreateResult(E resultType,
    IngredientInstance<E> a, IngredientInstance<E> b)
    {
        var clone = a.Clone();
        clone.Type = resultType;
        clone.Count = (a.Count + b.Count) / 2;
        clone.SelfCost = (a.SelfCost + b.SelfCost) / 2;
        return clone;
    }
}