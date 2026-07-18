using System;
using UnityEngine;
using DG.Tweening;

#region BASE

[Serializable]
public class DOTweenAnimationData
{
    [Header("Основные параметры")]
    public float Duration = 0.5f;
    public Ease Ease = Ease.Linear;
    public bool PlayOnStart = true;

    [Space, Header("Дополнительные параметры")]
    public float durationOnEnd = 0f;

    [HideInInspector] public Tweener lastTweener;

    [Space]
    public bool isKillLastTweener;
    public bool isKillAndCompleteLastTweener;
    public bool isNotPlayIsLastTweenerPlay;

    protected Tweener PlayTween(Func<Tweener> tweenFactory)
    {
        if (lastTweener != null && lastTweener.IsActive())
        {
            if (isNotPlayIsLastTweenerPlay && lastTweener.IsPlaying())
                return lastTweener;

            if (isKillAndCompleteLastTweener)
                lastTweener.Kill(true); // Complete + HitPlayer
            else if (isKillLastTweener)
                lastTweener.Kill();
        }

        lastTweener = tweenFactory.Invoke();

        if (durationOnEnd > 0f)
            lastTweener.SetDelay(durationOnEnd);

        return lastTweener;
    }
}

#endregion

#region POSITION

[Serializable]
public class DOTweenPositionData : DOTweenAnimationData
{
    [Header("Позиция")]
    public Vector3 TargetPosition;
    public bool IsRelative = false;
    public bool Snapping = false;

    [Header("Настройки")]
    public bool IsLocal = false;
    public bool useX = true;
    public bool useY = true;
    public bool usez = true;

    public Tweener TransformMove(Transform transform)
        => TransformMove(transform, TargetPosition);

    public Tweener TransformMove(Transform transform, Vector3 target)
    {
        return PlayTween(() =>
        {
            // Для относительного режима используем 0 для неиспользуемых осей,
            // для абсолютного — подставляем текущую координату для неиспользуемых осей
            Vector3 finalTarget;
            if (IsRelative)
            {
                finalTarget = new Vector3(
                    useX ? target.x : 0f,
                    useY ? target.y : 0f,
                    usez ? target.z : 0f
                );
            }
            else
            {
                var current = IsLocal ? transform.localPosition : transform.position;
                finalTarget = new Vector3(
                    useX ? target.x : current.x,
                    useY ? target.y : current.y,
                    usez ? target.z : current.z
                );
            }

            if (IsLocal)
                return transform.DOLocalMove(finalTarget, Duration, Snapping)
                                .SetEase(Ease)
                                .SetRelative(IsRelative);
            else
                return transform.DOMove(finalTarget, Duration, Snapping)
                                .SetEase(Ease)
                                .SetRelative(IsRelative);
        });
    }

    public Tweener PositionMove(Vector3 from, Vector3 to, Action<Vector3> onUpdate)
    {
        return PlayTween(() =>
            DOVirtual.Vector3(from, to, Duration, v => onUpdate?.Invoke(v))
                     .SetEase(Ease)
        );
    }

    public Tweener PositionMove(Vector3 to, Transform transform)
    {
        var from = IsLocal ? transform.localPosition : transform.position;
        return PositionMove(from, to, v =>
        {
            if (IsLocal)
                transform.localPosition = v;
            else
                transform.position = v;
        });
    }
}

#endregion

#region ROTATION

[Serializable]
public class DOTweenRotationData : DOTweenAnimationData
{
    [Header("Вращение")]
    public Vector3 TargetEulerAngles;
    public bool IsRelative = false;
    public RotateMode RotateMode = RotateMode.Fast;

    public Tweener TransformRotate(Transform transform)
        => TransformRotate(transform, TargetEulerAngles);

    public Tweener TransformRotate(Transform transform, Vector3 target)
    {
        return PlayTween(() =>
            transform.DORotate(target, Duration, RotateMode)
                     .SetEase(Ease)
                     .SetRelative(IsRelative)
        );
    }

    public Tweener RotationMove(Vector3 from, Vector3 to, Action<Vector3> onUpdate)
    {
        return PlayTween(() =>
            DOVirtual.Vector3(from, to, Duration, v => onUpdate?.Invoke(v))
                     .SetEase(Ease)
        );
    }

    public Tweener RotationMove(Vector3 to, Transform transform)
    {
        return RotationMove(transform.eulerAngles, to, v => transform.eulerAngles = v);
    }
}

#endregion

#region SCALE

[Serializable]
public class DOTweenScaleData : DOTweenAnimationData
{
    [Header("Масштаб")]
    public Vector3 TargetScale = Vector3.one;
    public bool IsRelative = false;

    public Tweener TransformScale(Transform transform)
        => TransformScale(transform, TargetScale);

    public Tweener TransformScale(Transform transform, Vector3 target)
    {
        return PlayTween(() =>
            transform.DOScale(target, Duration)
                     .SetEase(Ease)
                     .SetRelative(IsRelative)
        );
    }

    public Tweener ScaleMove(Vector3 from, Vector3 to, Action<Vector3> onUpdate)
    {
        return PlayTween(() =>
            DOVirtual.Vector3(from, to, Duration, v => onUpdate?.Invoke(v))
                     .SetEase(Ease)
        );
    }

    public Tweener ScaleMove(Vector3 to, Transform transform)
    {
        return ScaleMove(transform.localScale, to, v => transform.localScale = v);
    }
}

#endregion

#region PUNCH

[Serializable]
public class DOTweenPunchData : DOTweenAnimationData
{
    [Header("Punch")]
    public Vector3 Punch = Vector3.one;
    public int Vibrato = 10;
    public float Elasticity = 1f;

    public Tweener TransformPunchPosition(Transform transform)
    {
        return PlayTween(() =>
            transform.DOPunchPosition(Punch, Duration, Vibrato, Elasticity)
                     .SetEase(Ease)
        );
    }

    public Tweener TransformPunchRotation(Transform transform)
    {
        return PlayTween(() =>
            transform.DOPunchRotation(Punch, Duration, Vibrato, Elasticity)
                     .SetEase(Ease)
        );
    }

    public Tweener TransformPunchScale(Transform transform)
    {
        return PlayTween(() =>
            transform.DOPunchScale(Punch, Duration, Vibrato, Elasticity)
                     .SetEase(Ease)
        );
    }

    public Tweener PunchVector(Vector3 from, Vector3 to, Action<Vector3> onUpdate)
    {
        return PlayTween(() =>
            DOVirtual.Vector3(from, to, Duration, v => onUpdate?.Invoke(v))
                     .SetEase(Ease)
        );
    }
}

#endregion