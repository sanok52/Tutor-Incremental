using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;

public class ObstacleMoverSequenser : MonoBehaviour
{
    [SerializeField] private MoverBehaviourSequence[] behaviourSequences;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool playOnEnable = false;

    Tween tween = null;

    private void OnEnable()
    {
        if (playOnEnable == false)
            return;

        Stop();
        Play();
    }

    private void Start()
    {
        if (playOnStart)
            Play();
    }

    public void Play()
    {
        StartCoroutine(PlayCoroutine());
    }

    private IEnumerator PlayCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < behaviourSequences.Length; i++)
            {
                yield return StartCoroutine(PlaySequencesCoroutine(behaviourSequences[i]));
            }
            yield return null;
        }
    }

    private IEnumerator PlaySequencesCoroutine(MoverBehaviourSequence behaviourSequence)
    {
        // Найдём объект-цель по IDSpiker (имя GameObject)
        var target = transform;
        tween = null;

        switch (behaviourSequence.behaviourType)
        {
            case MoverBehaviourType.Wait:
                // Ничего не анимируем, просто ждём нужное время
                if (behaviourSequence.waitToPlay)
                {
                    yield return new WaitForSeconds(behaviourSequence.duration);
                }
                // если waitToPlay == false — сразу возвращаем управление
                yield break;

            case MoverBehaviourType.MovePosition:
                tween = target.DOMove(behaviourSequence.endPosition, behaviourSequence.duration)
                              .SetEase(behaviourSequence.ease)
                              .SetUpdate(UpdateType.Fixed);
                break;

            case MoverBehaviourType.MoveLocalPosition:
                tween = target.DOLocalMove(behaviourSequence.endLocalPosition, behaviourSequence.duration)
                              .SetEase(behaviourSequence.ease)
                              .SetUpdate(UpdateType.Fixed);
                break;

            case MoverBehaviourType.MoveXPosition:
                tween = target.DOMoveX(behaviourSequence.endX, behaviourSequence.duration)
                              .SetEase(behaviourSequence.ease)
                              .SetUpdate(UpdateType.Fixed);
                break;

            case MoverBehaviourType.MoveYPosition:
                tween = target.DOMoveY(behaviourSequence.endY, behaviourSequence.duration)
                              .SetEase(behaviourSequence.ease)
                              .SetUpdate(UpdateType.Fixed);
                break;

            case MoverBehaviourType.MoveZPosition:
                tween = target.DOMoveZ(behaviourSequence.endZ, behaviourSequence.duration)
                              .SetEase(behaviourSequence.ease)
                              .SetUpdate(UpdateType.Fixed);
                break;

            case MoverBehaviourType.Rotate:
                tween = target.DORotate(behaviourSequence.endRotation, behaviourSequence.duration)
                              .SetEase(behaviourSequence.ease)
                              .SetUpdate(UpdateType.Fixed);
                break;

            case MoverBehaviourType.Scale:
                tween = target.DOScale(behaviourSequence.endScale, behaviourSequence.duration)
                              .SetEase(behaviourSequence.ease)
                              .SetUpdate(UpdateType.Fixed);
                break;

            default:
                Debug.LogWarning($"[ObstacleMoverSequenser] Unsupported behaviourType '{behaviourSequence.behaviourType}'.");
                yield break;
        }

        if (behaviourSequence.waitToPlay)
        {
            // Ждём завершения твина
            yield return tween.WaitForCompletion();
        }
        // если waitToPlay == false, сразу возвращаем управление
    }

    internal void Stop()
    {
        StopAllCoroutines();
        tween?.Kill(false);
        enabled = false;
    }
}

public enum MoverBehaviourType
{
    Wait,
    MovePosition,
    MoveLocalPosition,
    MoveXPosition,
    MoveYPosition,
    MoveZPosition,
    Rotate,
    Scale
}

[System.Serializable]
public class MoverBehaviourSequence
{
    [Tooltip("Имя GameObject (IDSpiker), к которому применяется поведение")]
    public string ID;

    [Tooltip("Тип tween-операции")]
    public MoverBehaviourType behaviourType = MoverBehaviourType.Wait;

    [Tooltip("Длительность анимации или ожидания")]
    public float duration = 1f;

    [Tooltip("Ease для tween (например, Ease.OutQuad)")]
    public Ease ease = Ease.Linear;

    [Space]
    [Header("Параметры движения (World Space)")]
    public Vector3 endPosition = Vector3.zero;

    [Header("Параметры движения (Local Space)")]
    public Vector3 endLocalPosition = Vector3.zero;

    [Header("Параметры для осевых перемещений")]
    public float endX;
    public float endY;
    public float endZ;

    [Header("Параметры вращения и масштаба")]
    public Vector3 endRotation = Vector3.zero; // Euler angles
    public Vector3 endScale = Vector3.one;

    [Space]
    [Tooltip("Если true, следующий Sequence ждёт завершения этого tween; иначе запускается сразу")]
    public bool waitToPlay = true;
}