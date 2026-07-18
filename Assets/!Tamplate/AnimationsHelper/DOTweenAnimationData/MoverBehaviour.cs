using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum MoverBehaviourLoopType {
    OneDirection, //От начала до конца
    Repite, //От начала до конца, а потом снова
    PingPong //От начала до конца, потом в от конца в начало
}

public enum MoverBehaviourPlayType { //Поведение, если было выключено во время выполнения, а потом включено снова
    BreakAndNotReplay, //Прекратить воспоризведение и не начинать снова
    BreakAndReplay, //Прекратить воспоризведение, при включении начать заново
    PauseAndPlay  //Прекратить воспоризведение, при включении продолжить с места, где остановился
}

public class MoverBehaviour : MonoBehaviour
{
    public List<MoverIntruction> Intructions = new List<MoverIntruction>();

    [Space]
    public MoverBehaviourLoopType loopType = MoverBehaviourLoopType.Repite;
    public bool IsAutoPlayOnStart = true;
    public MoverBehaviourPlayType playType = MoverBehaviourPlayType.BreakAndNotReplay;

    // Внутренние поля для управления последовательностями
    private Sequence _mainSequence;
    private readonly List<Sequence> _stepSequences = new List<Sequence>();
    private bool _wasPlayingBeforeDisable;
    private bool _replayOnEnableRequested;
    private bool _initialized;

    private void Start()
    {
        _initialized = true;
        if (IsAutoPlayOnStart)
            Play();
    }

    private void OnDisable()
    {
        if (_mainSequence == null)
            return;

        // Если последовательность играла — реагируем в соответствии с playType
        if (_mainSequence.IsPlaying())
        {
            switch (playType)
            {
                case MoverBehaviourPlayType.BreakAndNotReplay:
                    Stop();
                    _wasPlayingBeforeDisable = false;
                    _replayOnEnableRequested = false;
                    break;
                case MoverBehaviourPlayType.BreakAndReplay:
                    Stop();
                    _replayOnEnableRequested = true;
                    _wasPlayingBeforeDisable = false;
                    break;
                case MoverBehaviourPlayType.PauseAndPlay:
                    Pause();
                    _wasPlayingBeforeDisable = true;
                    _replayOnEnableRequested = false;
                    break;
            }
        }
    }

    private void OnEnable()
    {
        if (!_initialized)
            return; // Start() ещё не вызван, автозапуск выполнится в Start

        if (_replayOnEnableRequested)
        {
            Play();
            return;
        }

        if (playType == MoverBehaviourPlayType.PauseAndPlay && _wasPlayingBeforeDisable)
        {
            // продолжить с места
            _mainSequence?.Play();
            _wasPlayingBeforeDisable = false;
        }
    }

    public void Play()
    {
        // Если уже есть активная последовательность и она играет — ничего не делаем
        if (_mainSequence != null && _mainSequence.IsActive() && _mainSequence.IsPlaying())
            return;

        BuildSequencesIfNeeded();
        _mainSequence?.Play();
        _wasPlayingBeforeDisable = true;
        _replayOnEnableRequested = false;
    }

    public void Stop()
    {
        // Полное остановление и уничтожение последовательностей
        if (_mainSequence != null)
        {
            _mainSequence.Kill(true);
            _mainSequence = null;
        }

        foreach (var s in _stepSequences)
        {
            if (s != null && s.IsActive())
                s.Kill(true);
        }
        _stepSequences.Clear();

        _wasPlayingBeforeDisable = false;
        _replayOnEnableRequested = false;
    }

    public void Pause()
    {
        if (_mainSequence != null && _mainSequence.IsActive())
            _mainSequence.Pause();
    }

    private void BuildSequencesIfNeeded()
    {
        // Если уже есть последовательность — пересоздадим (чтобы избежать повторов)
        if (_mainSequence != null)
        {
            _mainSequence.Kill();
            _mainSequence = null;
            foreach (var s in _stepSequences)
                s?.Kill();
            _stepSequences.Clear();
        }

        _mainSequence = DOTween.Sequence();
        _mainSequence.Pause();

        foreach (var instr in Intructions)
        {
            // Создаём шаг — последовательность, которая содержит все твины этого инструкта (выполняются параллельно)
            var step = DOTween.Sequence();
            step.Pause();

            bool hasAnyTween = false;

            List<Transform> targetsList = new List<Transform>();
            targetsList.AddRange(instr.targets);
            if (targetsList.Count == 0)
            {
                // Если нет указанных целей, добавляем сам объект
                targetsList.Add(transform);
            }

            foreach (var t in targetsList)
            {
                if (t == null)
                    continue;

                if (instr.UsePositionMove && instr.PositionData != null)
                {
                    var tw = instr.PositionData.TransformMove(t);
                    if (tw != null) { step.Join(tw); hasAnyTween = true; }
                }

                if (instr.UseRotationMove && instr.RotationData != null)
                {
                    var tw = instr.RotationData.TransformRotate(t);
                    if (tw != null) { step.Join(tw); hasAnyTween = true; }
                }

                if (instr.UseScale && instr.ScaleData != null)
                {
                    var tw = instr.ScaleData.TransformScale(t);
                    if (tw != null) { step.Join(tw); hasAnyTween = true; }
                }

                if (instr.UsePunchPosition && instr.PunchDataPosition != null)
                {
                    var tw = instr.PunchDataPosition.TransformPunchPosition(t);
                    if (tw != null) { step.Join(tw); hasAnyTween = true; }
                }

                if (instr.UsePunchRotate && instr.PunchDataRotate != null)
                {
                    var tw = instr.PunchDataRotate.TransformPunchRotation(t);
                    if (tw != null) { step.Join(tw); hasAnyTween = true; }
                }

                if (instr.UsePunchScale && instr.PunchDataScale != null)
                {
                    var tw = instr.PunchDataScale.TransformPunchScale(t);
                    if (tw != null) { step.Join(tw); hasAnyTween = true; }
                }
            }

            if (!hasAnyTween)
            {
                // Нет твинов в этом шаге — пропускаем
                continue;
            }

            _stepSequences.Add(step);

            if (instr.notWaitEndAnimation)
            {
                // Запускаем шаг параллельно — добавляем callback, который стартует шаg, но основной _mainSequence не будет ждать его окончания
                _mainSequence.AppendCallback(() => step.Play());
            }
            else
            {
                // Обычное последовательное выполнение — mainSequence будет ждать завершения step
                _mainSequence.Append(step);
            }
        }

        // Настраиваем тип цикления для главной последовательности
        switch (loopType)
        {
            case MoverBehaviourLoopType.OneDirection:
                _mainSequence.SetLoops(1, LoopType.Restart);
                break;
            case MoverBehaviourLoopType.Repite:
                _mainSequence.SetLoops(-1, LoopType.Restart); // бесконечный повтор
                break;
            case MoverBehaviourLoopType.PingPong:
                _mainSequence.SetLoops(-1, LoopType.Yoyo); // бесконечный йойо
                break;
        }
    }
}

[Serializable]
public class MoverIntruction
{
    public Transform[] targets;

    [Space, Header("DOMove")]
    public bool UsePositionMove;
    public DOTweenPositionData PositionData;

    [Space, Header("DORotate")]
    public bool UseRotationMove;
    public DOTweenRotationData RotationData;

    [Space, Header("DOScale")]
    public bool UseScale;
    public DOTweenScaleData ScaleData;

    [Space, Header("DOPunchPosition")]
    public bool UsePunchPosition;
    public DOTweenPunchData PunchDataPosition;

    [Space, Header("DOPunchRotate")]
    public bool UsePunchRotate;
    public DOTweenPunchData PunchDataRotate;

    [Space, Header("DOPunchScale")]
    public bool UsePunchScale;
    public DOTweenPunchData PunchDataScale;

    [Space(10)]
    public bool notWaitEndAnimation;
}