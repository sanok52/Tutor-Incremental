using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class UIPunchFX : MonoBehaviour
{
    [SerializeField] private UIPunchInstruction[] instructions;

    private readonly Dictionary<string, List<UIPunchInstruction>> dicInstructions = new();
    private readonly Dictionary<Transform, Tween> scaleTweens = new();
    private readonly Dictionary<Transform, Tween> positionTweens = new();
    private readonly Dictionary<Transform, Tween> rotationTweens = new();

    private float? cachedCoefficient = null;

    private void Start()
    {
        foreach (var instr in instructions)
        {
            if (!dicInstructions.ContainsKey(instr.ID))
                dicInstructions[instr.ID] = new List<UIPunchInstruction>();

            dicInstructions[instr.ID].Add(instr);
        }
    }

    public void SetPunchCoefficient(float coefficient)
    {
        cachedCoefficient = coefficient;
    }

    public void InvokeInstructions(string id)
    {
        if (!dicInstructions.TryGetValue(id, out var list)) return;

        float value = cachedCoefficient ?? 1f;
        cachedCoefficient = null;

        foreach (var instr in list)
        {
            value = Mathf.Clamp(value, -instr.affectMultiplierMax, instr.affectMultiplierMax);

            foreach (var target in instr.targets)
            {
                if (target == null) continue;

                // Scale
                if (instr.isUseScalePunch)
                {
                    if (scaleTweens.TryGetValue(target, out var tween))
                    {
                        tween.Kill();
                        target.localScale = Vector3.one;
                    }

                    Vector3 scaleValue = instr.isScaleAffectedByValue
                        ? instr.scaleSize * (instr.scaleAffectMultiplier * value)
                        : instr.scaleSize;

                    //Debug.Log($"{instr.scaleSize * (instr.scaleAffectMultiplier * value)} => {instr.scaleSize} * ({instr.scaleAffectMultiplier} * {value})");
                    var newTween = target.DOPunchScale(scaleValue, instr.scaleDuration, instr.scaleVibration).SetEase(Ease.OutQuad);
                    scaleTweens[target] = newTween;
                }

                // Position
                if (instr.isUsePositionPunch)
                {
                    if (positionTweens.TryGetValue(target, out var tween))
                    {
                        tween.Kill();
                        target.localPosition = Vector3.zero;
                    }

                    Vector3 posValue = instr.isPositionAffectedByValue
                        ? instr.positionAffectMultiplier * value * Vector3.one
                        : instr.positionSize;

                    var newTween = target.DOPunchPosition(posValue, instr.positionDuration, instr.positionVibration).SetEase(Ease.OutQuad);
                    positionTweens[target] = newTween;
                }

                // Rotation
                if (instr.isUseRotationPunch)
                {
                    if (rotationTweens.TryGetValue(target, out var tween))
                    {
                        tween.Kill();
                        target.localRotation = Quaternion.identity;
                    }

                    Vector3 rotValue = instr.isRotationAffectedByValue
                        ? instr.rotationAffectMultiplier * value * Vector3.one
                        : instr.rotationSize;

                    var newTween = target.DOPunchRotation(rotValue, instr.rotationDuration, instr.rotationVibration).SetEase(Ease.OutQuad);
                    rotationTweens[target] = newTween;
                }
            }
        }
    }
}

[System.Serializable]
public class UIPunchInstruction
{
    public string ID;
    public Transform[] targets;

    [Header("Scale")]
    public bool isUseScalePunch;
    public Vector3 scaleSize = Vector3.one * 0.2f;
    public float scaleDuration = 0.3f;
    public int scaleVibration = 10;
    public bool isScaleAffectedByValue;
    public float scaleAffectMultiplier = 1f;

    [Header("Position")]
    public bool isUsePositionPunch;
    public Vector3 positionSize = Vector3.one * 10f;
    public float positionDuration = 0.3f;
    public int positionVibration = 10;
    public bool isPositionAffectedByValue;
    public float positionAffectMultiplier = 10f;

    [Header("Rotation")]
    public bool isUseRotationPunch;
    public Vector3 rotationSize = Vector3.one * 10f;
    public float rotationDuration = 0.3f;
    public int rotationVibration = 10;
    public bool isRotationAffectedByValue;
    public float rotationAffectMultiplier = 10f;

    [Space]
    public float affectMultiplierMax = 100f;
}