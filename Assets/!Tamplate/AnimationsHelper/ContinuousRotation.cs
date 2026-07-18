using UnityEngine;
using DG.Tweening;

public class ContinuousRotation : MonoBehaviour
{
    [Header("Оси вращения (градусы в секунду)")]
    public Vector3 rotationSpeed = new Vector3(0, 90, 0); // Например, вращение по Y со скоростью 90°/сек

    private Tween rotationTween;

    void Start()
    {
        StartRotation();
    }

    public void StartRotation()
    {
        // Расчёт целевого угла через 1 секунду, на который нужно повернуть объект
        Vector3 endRotation = transform.eulerAngles + rotationSpeed;

        rotationTween = transform.DORotate(endRotation, 1f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }

    public void StopRotation()
    {
        if (rotationTween != null && rotationTween.IsActive())
            rotationTween.Kill();
    }

    void OnDestroy()
    {
        StopRotation();
    }
}