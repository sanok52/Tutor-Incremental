using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RecordData
{
    public string nick;
    public int score;

    public RecordData(string n, int s)
    {
        nick = n;
        score = s;
    }
}

[Serializable]          // оболочка, потому что JsonUtility не умеет List<> «голым»
class RecordsWrapper
{
    public List<RecordData> records = new List<RecordData>();
}

public static class LeaderBoard
{
    private const string PREF_KEY = "Records";
    private const int MAX_ROWS = 10;

    public static List<RecordData> recordDatas = new List<RecordData>();

    /*--------------------------------------------------------------*/
    public static void SaveScore(string nick, int youScore)
    {
        // 2-3  — добавляем либо обновляем запись
        RecordData existing = recordDatas.FirstOrDefault(r => r.nick == nick);
        if (existing != null)
        {
            if (youScore > existing.score)                       // обновляем, если лучше
                existing.score = youScore;
        }
        else
        {
            recordDatas.Add(new RecordData(nick, youScore));
        }

        // 4  — сортировка и обрезка
        recordDatas = recordDatas
                     .OrderByDescending(r => r.score)
                     .Take(MAX_ROWS)
                     .ToList();

        // 5  — сериализация и сохранение
        var wrapper = new RecordsWrapper { records = recordDatas };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PREF_KEY, json);
        PlayerPrefs.Save();
    }

    public static void SaveScore(int youScore)
    {
        SaveScore("default", youScore);
    }

    public static RecordData GetScore (string nick)
    {
        RecordData existing = recordDatas.FirstOrDefault(r => r.nick == nick);
        return existing;
    }

    /*--------------------------------------------------------------*/
    public static void LoadRecords()
    {
        recordDatas.Clear();

        if (!PlayerPrefs.HasKey(PREF_KEY)) return;

        string json = PlayerPrefs.GetString(PREF_KEY, "");
        if (string.IsNullOrEmpty(json)) return;

        RecordsWrapper wrapper = JsonUtility.FromJson<RecordsWrapper>(json);
        if (wrapper != null && wrapper.records != null)
            recordDatas = wrapper.records;
    }

    // --------------------------------------------------------------
    /// <summary>Возвращает наибольший сохранённый счёт. Если таблица пуста – 0.</summary>
    public static int GetBestScore()
    {
        // Убедимся, что данные загружены
        if (recordDatas == null || recordDatas.Count == 0)
            LoadRecords();

        return (recordDatas.Count > 0) ? recordDatas.Max(r => r.score) : 0;
    }

    // --------------------------------------------------------------
    /// <summary>
    /// Возвращает TOP-список рекордов (до requestedCount строк).
    /// Если запрошено больше, чем есть, вернёт сколько есть.
    /// </summary>
    public static List<RecordData> GetBestScoreList(int requestedCount)
    {
        if (requestedCount <= 0) return new List<RecordData>();

        // Убедимся, что данные загружены
        if (recordDatas == null || recordDatas.Count == 0)
            LoadRecords();

        // Уже отсортированы в SaveScore, но на всякий случай
        return recordDatas
               .OrderByDescending(r => r.score)
               .Take(requestedCount)
               .ToList();
    }
}