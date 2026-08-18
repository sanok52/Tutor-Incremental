using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI-элемент, отображающий все цели одной миссии.
/// </summary>
public class MissionUI : MonoBehaviour
{
    [SerializeField] private MissionTargetUI missionTargetUIPref; // Префаб цели.
    [SerializeField] private Transform container;                 // Контейнер для целей.

    private List<MissionTargetUI> missionTargetUIs = new List<MissionTargetUI>();
    public Mission Mission { get; private set; }

    /// <summary>
    /// Инициализация: создаёт UI для каждой цели.
    /// </summary>
    public void Init(Mission mission)
    {
        Mission = mission;
        foreach (var target in mission.Targets)
        {
            var targetUI = Instantiate(missionTargetUIPref, container);
            targetUI.Init(target);
            missionTargetUIs.Add(targetUI);
        }
        UpdateUI(); // Первичное обновление состояния.
    }

    /// <summary>
    /// Обновляет состояние всех целей в соответствии с прогрессом миссии.
    /// Текущая фаза (активная цель) выделяется, если миссия ещё не завершена.
    /// </summary>
    public void UpdateUI()
    {
        if (Mission == null) return;

        for (int i = 0; i < Mission.Targets.Length && i < missionTargetUIs.Count; i++)
        {
            // Текущая цель — это та, индекс которой равен Phase, но только если миссия не завершена.
            bool isCurrent = !Mission.IsCompleted && (i == Mission.Phase);
            // Цель считается завершённой, если её индекс меньше текущей фазы или миссия полностью завершена.
            bool isCompleted = (i < Mission.Phase) || Mission.IsCompleted;

            missionTargetUIs[i].UpdateProgress(Mission.Targets[i].Count, isCurrent, isCompleted);
        }
    }

    /// <summary>
    /// Принудительно помечает все цели как завершённые и снимает выделение.
    /// Вызывается, когда миссия полностью выполнена.
    /// </summary>
    public void SetCompleted()
    {
        foreach (var ui in missionTargetUIs)
            ui.SetCompleted();
    }

    /// <summary>
    /// Запускает анимацию удаления элемента миссии.
    /// После завершения анимации вызывается переданный callback.
    /// </summary>
    public void PlayRemoveAnimation(Action onComplete)
    {
        // Здесь можно реализовать анимацию (например, с помощью DOTween, LeanTween или стандартного Animator).
        // По окончании анимации вызываем onComplete().
        // Для примера вызываем сразу.
        onComplete?.Invoke();
    }
}