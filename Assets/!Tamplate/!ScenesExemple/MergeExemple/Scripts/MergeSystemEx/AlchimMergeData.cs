using UnityEngine;
// ========================= педюйрнп дкъ йнмйпермнцн рхою (AlchimElements) =========================

[CreateAssetMenu(menuName = "Merge/Alchim Data")]
public class AlchimMergeData : MergeData<
    AlchimElements,
    IngredientData<AlchimElements>,
    AlchimIngredientInstance,
    Recipe<AlchimElements, AlchimIngredientInstance>>
{
}

public enum AlchimElements { None = 0, Fire, Water, Earth, Wind, Steam }