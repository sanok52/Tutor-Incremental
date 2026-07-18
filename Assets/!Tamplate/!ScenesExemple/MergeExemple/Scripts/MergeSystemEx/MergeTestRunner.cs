using System.Linq;
using UnityEngine;

public class MergeTestRunner : MonoBehaviour
{
    [SerializeField] private AlchimMergeData mergeData;
    [SerializeField] private AlchimIngredientInstance inputA;
    [SerializeField] private AlchimIngredientInstance inputB;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            AlchimElements element = mergeData.GetResult(inputA, inputB, out AlchimIngredientInstance result, out var recipe);

            Debug.Log($"{result != null}");
            if (recipe == null)
                return;

            Debug.Log($"Type => {result.Type}");
            Debug.Log($"Count => {result.Count}, Purity => {result.Purity}, SelfCost => {result.SelfCost}");
            Debug.Log($"result is {mergeData.AllIngredients.FirstOrDefault(x => x.Type == result.Type).Title}");
        }
    }
}