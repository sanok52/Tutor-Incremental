using UnityEngine;

[CreateAssetMenu(menuName = "Merge/Behaviours/PurityGreater")]
public class PurityGreaterBehaviour : MergeBehaviour<AlchimElements, AlchimIngredientInstance>
{
    public int Border = 50;

    public override AlchimElements GetResult(
        Recipe<AlchimElements, AlchimIngredientInstance> recipe,
        AlchimIngredientInstance inputA,
        AlchimIngredientInstance inputB,
        AlchimElements[] specialResults,
        out AlchimIngredientInstance resultInstance,
        out bool breakChain)
    {
        breakChain = false;
        resultInstance = null;
        if (specialResults == null || specialResults.Length < 2) return recipe.SimpleResult;

        int minPurity = Mathf.Min(inputA.Purity, inputB.Purity);
        AlchimElements result = minPurity > Border ? specialResults[1] : specialResults[0];

        resultInstance = new AlchimIngredientInstance
        {
            Type = result,
            Count = (inputA.Count + inputB.Count) / 2,
            SelfCost = (inputA.SelfCost + inputB.SelfCost) / 2,
            Purity = (inputA.Purity + inputB.Purity) / 2
        };
        return result;
    }

    public override string[] GetSpecialResultTitles() => new[] { "Less/Equal", "Greater" };
}