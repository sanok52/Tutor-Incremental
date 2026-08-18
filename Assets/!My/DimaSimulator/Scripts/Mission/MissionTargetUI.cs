using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionTargetUI : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpTag;
    [SerializeField] private Image imgSelect;
    [SerializeField] private Image imgComplite;
    [SerializeField] private TMP_Text tmpCount;

    private MissionTarget target;

    public void Init(MissionTarget missionTarget)
    {
        target = missionTarget;
        tmpTag.text = "#" + MissionManager.GetTagName(missionTarget.TargetTag);
        SetComplite(false);
        SetSelect(false);
        SetCount(0);
    }

    public void UpdateProgress(int currentCount, bool isCurrent, bool isCompleted)
    {
        SetCount(currentCount);
        SetSelect(isCurrent);
        SetComplite(isCompleted);
    }

    public void SetCompleted()
    {
        SetComplite(true);
        SetSelect(false);
    }

    private void SetCount(int count)
    {
        tmpCount.text = count > 0 ? count.ToString() : "";
    }

    private void SetSelect(bool isSelect)
    {
        imgSelect.enabled = isSelect;
    }

    private void SetComplite(bool isComplite)
    {
        imgComplite.enabled = isComplite;
    }
}