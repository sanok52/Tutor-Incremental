using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreSystem
{
    public int Score { get; private set; }

    public event Action<ChangeScoreInfo> OnScoreChange;
    public event Action<ChangeScoreInfo> OnAddScore;
    public event Action<ChangeScoreInfo> OnRemoveScore;

    public ScoreSystem()
    {
        SetScore(0, Vector3.zero);
    }

    public ScoreSystem(int score)
    {
        SetScore(score, Vector3.zero);
    }

    public void AddScore(int add, Vector3 point = default)
    {
        AddScore(new ChangeScoreInfo(add, point)
        {
            ChangeType = ChangeScoreType.Add
        });
    }

    public void AddScore(ChangeScoreInfo changeInfo)
    {
        changeInfo.ChangeType = ChangeScoreType.Add;

        int add = changeInfo.Value;

        if (add < 0)
        {
            RemoveScore(new ChangeScoreInfo(changeInfo)
            {
                Value = -add,
                ChangeType = ChangeScoreType.Remove
            });
            return;
        }

        var setInfo = new ChangeScoreInfo(changeInfo)
        {
            Value = Score + add,
            ChangeType = ChangeScoreType.Add
        };

        SetScore(setInfo);
        OnAddScore?.Invoke(changeInfo);
    }

    public void RemoveScore(int remove, Vector3 point = default)
    {
        RemoveScore(new ChangeScoreInfo(remove, point)
        {
            ChangeType = ChangeScoreType.Remove
        });
    }

    public void RemoveScore(ChangeScoreInfo changeInfo)
    {
        changeInfo.ChangeType = ChangeScoreType.Remove;

        int remove = changeInfo.Value;

        if (remove < 0)
        {
            AddScore(new ChangeScoreInfo(changeInfo)
            {
                Value = -remove,
                ChangeType = ChangeScoreType.Add
            });
            return;
        }

        var setInfo = new ChangeScoreInfo(changeInfo)
        {
            Value = Score - remove,
            ChangeType = ChangeScoreType.Remove
        };

        SetScore(setInfo);
        OnRemoveScore?.Invoke(changeInfo);
    }

    public void SetScore(int score, Vector3 point = default)
    {
        SetScore(new ChangeScoreInfo(score, point)
        {
            ChangeType = ChangeScoreType.Set
        });
    }

    public void SetScore(ChangeScoreInfo changeInfo)
    {
        changeInfo.ChangeType = ChangeScoreType.Set;

        int delta = changeInfo.Value - Score;

        Score = changeInfo.Value;
        changeInfo.Delta = delta;

        OnScoreChange?.Invoke(changeInfo);
    }

}

public enum ChangeScoreType { Set, Add, Remove }
public class ChangeScoreInfo
{
    public int Value;
    public Vector3 Point;
    public string Prefix;
    public string Postfix;

    [Space]
    public int? Delta;
    public ChangeScoreType ChangeType = ChangeScoreType.Set;

    public ChangeScoreInfo()
    {
    }

    public ChangeScoreInfo(ChangeScoreInfo clone)
    {
        Value = clone.Value;
        Point = clone.Point;
        Prefix = clone.Prefix;
        Postfix = clone.Postfix;
        Delta = clone.Delta;
        ChangeType = clone.ChangeType;
    }

    public ChangeScoreInfo(int value, Vector3 point)
    {
        Value = value;
        Point = point;
    }
}