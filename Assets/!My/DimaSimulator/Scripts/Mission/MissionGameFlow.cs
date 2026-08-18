using System.Collections;
using UnityEngine;

public class MissionGameFlow : MonoBehaviour
{
    [SerializeField] private int minimalMissions = 1;
    private MissionManager missionManager;

    private void Start()
    {
        missionManager = G.MissionManager;
        SubscribeUI();
        StartCoroutine(GameFlowRoutine());
    }

    private void SubscribeUI()
    {
        MissionWindiowUI missionWindow = FindFirstObjectByType<MissionWindiowUI>();
        missionManager.OnMissionAdd += missionWindow.AddMission;
        missionManager.OnMissionUpdate += missionWindow.UpdateMission;
        missionManager.OnMissionComplite += missionWindow.MissionComplite;
        missionManager.OnMissionRemove += missionWindow.RemoveMission;

        G.PlayerInGame.OnPlayerPostReaction += PlayerPostReactionWork;
        missionManager.OnMissionComplite += MissionCompliteWork;
    }

    private IEnumerator GameFlowRoutine()
    {
        while (true)
        {
            while (missionManager.Missions.Length < minimalMissions)
            {
                missionManager.CreateRandomMission();
                yield return new WaitForSeconds(2f);
            }

            yield return new WaitUntil(() => missionManager.HasCompletedMission);

            // ћисси€ будет удалена автоматически через задержку, так что здесь ничего не делаем
            yield return new WaitForSeconds(1f);
        }
    }

    private void PlayerPostReactionWork(Post post, ReactionType type, int count)
    {
        missionManager.TestAllMissonReaction(post, type, count);
    }

    private void MissionCompliteWork(Mission mission)
    {
        G.PlayerInGame.AddLikesForMission(mission);
    }
}