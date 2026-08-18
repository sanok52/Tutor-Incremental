using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionWindiowUI : MonoBehaviour
{
    [SerializeField] private MissionUI missionUIPref;
    [SerializeField] private Transform container;

    private List<MissionUI> missionUIs = new List<MissionUI>();

    public void AddMission(Mission mission)
    {
        var ui = Instantiate(missionUIPref, container);
        ui.Init(mission);
        missionUIs.Add(ui);
    }

    public void UpdateMission(Mission mission)
    {
        var ui = missionUIs.Find(m => m.Mission == mission);
        if (ui != null)
            ui.UpdateUI();
    }

    public void MissionComplite(Mission mission)
    {
        var ui = missionUIs.Find(m => m.Mission == mission);
        if (ui != null)
            ui.SetCompleted();
        // Не удаляем сразу – ждём вызова RemoveMission после анимации
    }

    public void RemoveMission(Mission mission)
    {
        var ui = missionUIs.Find(m => m.Mission == mission);
        if (ui != null)
        {
            missionUIs.Remove(ui);
            // Здесь можно запустить анимацию удаления, а затем уничтожить
            ui.PlayRemoveAnimation(() => Destroy(ui.gameObject));
        }
    }
}