using System;

[Serializable]
public class AlchimIngredientInstance : IngredientInstance<AlchimElements>
{
    public int Purity;

    public override IngredientInstance<AlchimElements> Clone() => new AlchimIngredientInstance
    {
        Type = Type,
        Count = Count,
        SelfCost = SelfCost,
        Purity = Purity
    };

    public override IngredientInstance<AlchimElements> CreateResult(
    AlchimElements resultType,
    IngredientInstance<AlchimElements> a,
    IngredientInstance<AlchimElements> b)
    {
        var alchA = a as AlchimIngredientInstance;
        var alchB = b as AlchimIngredientInstance;
        if (alchA == null || alchB == null)
            return base.CreateResult(resultType, a, b);
        return new AlchimIngredientInstance
        {
            Type = resultType,
            Count = (alchA.Count + alchB.Count) / 2,
            SelfCost = (alchA.SelfCost + alchB.SelfCost) / 2,
            Purity = (alchA.Purity + alchB.Purity) / 2
        };
    }
}