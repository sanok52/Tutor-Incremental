using System.Collections;
using UnityEngine;
using TMPro;

public class GameTimerText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    public int TargetValue;
    private GameTimer gameTimer;

    [Space]
    [SerializeField] private string prefix;
    [SerializeField] private string postfix;

    public void Init(GameTimer gameTimer)
    {
        if (gameTimer != null)
            gameTimer.OnTick -= SetTimeText;
        this.gameTimer = gameTimer;
        gameTimer.OnTick += SetTimeText;
    }

    public void SetTimeText(int seconds)
    {
        tmp.text = prefix +
            GameTimer.GetFormattedTime(seconds) + (TargetValue > 0 ? $"/{GameTimer.GetFormattedTime(TargetValue)}" : $"") +
            postfix;
    }

    private void OnDestroy()
    {
        if(gameTimer != null)
            gameTimer.OnTick -= SetTimeText;
    }
}