using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MissionManager
{
    private List<Mission> missions = new List<Mission>();

    public event Action<Mission> OnMissionAdd;
    public event Action<Mission> OnMissionUpdate;
    public event Action<Mission> OnMissionComplite;
    public event Action<Mission> OnMissionRemove;

    public bool HasCompletedMission => missions.Any(m => m.IsCompleted);

    public Mission[] Missions => missions.ToArray();

    public static string GetTagName(string targetTag) => targetTag;

    public void CreateRandomMission()
    {
        var player = G.PlayerInGame;
        if (player == null || player.subscribedAuthors.Count == 0)
            return;

        List<MissionTarget> missionTargets = new List<MissionTarget>();
        int count = Random.Range(2, 5);
        for (int i = 0; i < count; i++)
        {
            var author = player.subscribedAuthors.ToArray().RandomElement();
            var post = author.possiblePosts.ToArray().RandomElement();
            if (post.HashTags == null || post.HashTags.Count == 0 )
            {
                i--;
                continue;
            }
            string tag = post.HashTags.ToArray().RandomElement();

            if (missionTargets.Count > 0 && missionTargets[missionTargets.Count - 1].TargetTag == tag)
            {
                i--;
                continue;
            }

            missionTargets.Add(new MissionTarget { TargetTag = tag, Count = 0 });
        }

        if (missionTargets.Count == 0) return;

        var mission = new Mission
        {
            Phase = 0,
            Targets = missionTargets.ToArray()
        };

        AddMission(mission);
    }

    public void AddMission(Mission mission)
    {
        // Подписываемся на события миссии
        mission.OnTargetProgressChanged += (index) => OnMissionUpdate?.Invoke(mission);
        mission.OnPhaseChanged += (newPhase) => OnMissionUpdate?.Invoke(mission);
        mission.OnCompleted += () =>
        {
            OnMissionComplite?.Invoke(mission);
            // Запускаем отложенное удаление (например, через 5 секунд, чтобы UI успел показать анимацию)
            ScheduleMissionRemoval(mission, 5f);
        };

        missions.Add(mission);
        OnMissionAdd?.Invoke(mission);
    }

    private void ScheduleMissionRemoval(Mission mission, float delay)
    {
        // Используем MonoBehaviour-корутину (можно создать отдельный объект-менеджер или использовать GameFlow)
        // Для простоты создадим временный объект, который выполнит удаление
        GameObject holder = new GameObject("MissionRemovalScheduler");
        var coroutineRunner = holder.AddComponent<MissionRemovalCoroutine>();
        coroutineRunner.StartCoroutine(coroutineRunner.RemoveAfterDelay(mission, delay, () =>
        {
            RemoveMission(mission);
            UnityEngine.Object.Destroy(holder);
        }));
    }

    // Вспомогательный класс для корутины
    private class MissionRemovalCoroutine : MonoBehaviour
    {
        public System.Collections.IEnumerator RemoveAfterDelay(Mission mission, float delay, Action onComplete)
        {
            yield return new WaitForSeconds(delay);
            onComplete?.Invoke();
        }
    }

    public void RemoveMission(Mission mission)
    {
        // Отписываемся от событий (опционально, но хорошая практика)
        //mission.OnTargetProgressChanged = null;
        //mission.OnPhaseChanged = null;
        //mission.OnCompleted = null;

        if (missions.Remove(mission))
        {
            OnMissionRemove?.Invoke(mission);
        }
    }

    public void TestAllMissonReaction(Post post, ReactionType type, int count)
    {
        var tags = post.HashTags;

        foreach (var mission in missions.ToArray())
        {
            if (mission.IsCompleted)
                continue;

            if (tags.Contains(mission.Targets[mission.Phase].TargetTag))
            {
                mission.AddInCurrentPhase(count);
                mission.LastPost = post;
                continue;
            }

            if (mission.LastPost == post)
            {
                mission.AddInLastPhase(count);
            }
        }
    }
}