using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogDirector : MonoBehaviour
{
    [SerializeField] private DialogPlayer dialogPlayer;
    private List<QueuedScene> sceneQueue = new List<QueuedScene>();
    private QueuedScene currentQueuedScene;

    public event Action<DialogScene> OnSceneStarted;
    public event Action<DialogScene> OnSceneEnded;
    public event Action<DialogScene> OnSceneSkipped; // сцена удалена (LostIsBreak или kill)
    public bool IsPlaying => dialogPlayer.IsPlaying;

    private void Awake()
    {
        if (dialogPlayer == null)
            dialogPlayer = GetComponent<DialogPlayer>();
        if (dialogPlayer == null)
            Debug.LogError("DialogDirector: No DialogPlayer assigned!");
    }

    private void OnEnable()
    {
        if (dialogPlayer != null)
        {
            dialogPlayer.OnDialogStarted += OnPlayerDialogStarted;
            dialogPlayer.OnDialogEnded += OnPlayerDialogEnded;
        }
    }

    private void OnDisable()
    {
        if (dialogPlayer != null)
        {
            dialogPlayer.OnDialogStarted -= OnPlayerDialogStarted;
            dialogPlayer.OnDialogEnded -= OnPlayerDialogEnded;
        }
    }

    private void OnPlayerDialogStarted(DialogScene scene)
    {
        // синхронизация
        if (currentQueuedScene == null || currentQueuedScene.Scene != scene)
            currentQueuedScene = new QueuedScene(scene, scene.Priority);
    }

    private void OnPlayerDialogEnded(DialogScene scene)
    {
        // нормальное завершение
        if (currentQueuedScene != null && currentQueuedScene.Scene == scene)
        {
            sceneQueue.Remove(currentQueuedScene);
            currentQueuedScene = null;
        }
        OnSceneEnded?.Invoke(scene);
        PlayNextInQueue();
    }

    private void PlayNextInQueue()
    {
        if (sceneQueue.Count == 0) return;
        var next = sceneQueue[0];
        sceneQueue.RemoveAt(0);
        PlaySceneImmediately(next);
    }

    private void KillScenes(string[] killNames)
    {
        if (killNames == null || killNames.Length == 0) return;

        // Удаляем из очереди
        sceneQueue.RemoveAll(q => killNames.Contains(q.Scene.name));

        // Если текущая сцена совпадает – прерываем её
        if (currentQueuedScene != null && killNames.Contains(currentQueuedScene.Scene.name))
        {
            OnSceneSkipped?.Invoke(currentQueuedScene.Scene);
            currentQueuedScene = null;
            if (dialogPlayer.IsPlaying)
                dialogPlayer.StopCurrentScene();
        }
    }

    private void PlaySceneImmediately(QueuedScene queued)
    {
        if (dialogPlayer == null) return;

        KillScenes(queued.Scene.killDialogScenes);

        // Если плеер занят – нужно прервать текущую сцену
        if (dialogPlayer.IsPlaying)
        {
            // Определяем текущую сцену
            DialogScene currentScene = currentQueuedScene?.Scene;
            if (currentScene == null && dialogPlayer.CurrentPlayingScene != null)
            {
                currentScene = dialogPlayer.CurrentPlayingScene;
                currentQueuedScene = new QueuedScene(currentScene, currentScene.Priority);
            }

            if (currentScene != null)
            {
                // Проверяем, не убивает ли новая сцена текущую через kill-список
                if (queued.Scene.killDialogScenes != null &&
                    Array.IndexOf(queued.Scene.killDialogScenes, currentScene.name) >= 0)
                {
                    sceneQueue.RemoveAll(q => q.Scene == currentScene);
                    OnSceneSkipped?.Invoke(currentScene);
                    // текущая сцена будет удалена, не возвращаем в очередь
                }
                else
                {
                    // Обычная логика LostIsBreak
                    if (currentScene.LostIsBreak)
                    {
                        sceneQueue.RemoveAll(q => q.Scene == currentScene);
                        OnSceneSkipped?.Invoke(currentScene);
                    }
                    else
                    {
                        // возвращаем в начало очереди
                        var requeue = new QueuedScene(currentScene, currentQueuedScene.Priority);
                        InsertAtFront(requeue);
                    }
                }
            }

            // Вставляем новую сцену в начало очереди (она будет следующей после остановки)
            InsertAtFront(queued);
            dialogPlayer.StopCurrentScene();
            return;
        }

        // Ничего не играет – запускаем сразу
        currentQueuedScene = queued;
        OnSceneStarted?.Invoke(queued.Scene);
        dialogPlayer.PlayScene(queued.Scene);
    }

    private void InsertAtFront(QueuedScene scene)
    {
        sceneQueue.Insert(0, scene);
    }

    public void AddScene(DialogScene scene, int priority = -1)
    {
        if (scene == null) return;
        int effectivePriority = priority == -1 ? scene.Priority : priority;
        var queued = new QueuedScene(scene, effectivePriority);

        if (queued == null || queued.Scene == null)
        {
            Debug.Log($"Невозможно воспроизвести queued {(queued != null ? "queued.Scene == null" : "queued == null")}");
            return;
        }

        KillScenes(scene.killDialogScenes);

        if (!dialogPlayer.IsPlaying)
        {
            PlaySceneImmediately(queued);
            return;
        }

        int currentPriority = -1;
        if (currentQueuedScene != null)
            currentPriority = currentQueuedScene.Priority;
        else if (dialogPlayer.CurrentPlayingScene != null)
            currentPriority = dialogPlayer.CurrentPlayingScene.Priority;

        bool shouldInterrupt = false;
        if (effectivePriority > currentPriority)
            shouldInterrupt = true;
        else if (effectivePriority == currentPriority)
            shouldInterrupt = scene.BreakEqualPriority;

        if (shouldInterrupt)
        {
            PlaySceneImmediately(queued);
        }
        else
        {
            if (scene.LostIsNotPlay)
            {
                OnSceneSkipped?.Invoke(scene);
                return;
            }
            InsertWithPriority(queued);
        }
    }

    public void AddSceneForce(DialogScene scene)
    {
        if (scene == null) return;
        var queued = new QueuedScene(scene, int.MaxValue);
        PlaySceneImmediately(queued);
    }

    private void InsertWithPriority(QueuedScene newScene)
    {
        int index = 0;
        while (index < sceneQueue.Count && sceneQueue[index].Priority >= newScene.Priority)
            index++;
        sceneQueue.Insert(index, newScene);
    }

    public void ClearQueue()
    {
        sceneQueue.Clear();
    }

    public void StopCurrentAndClear()
    {
        if (dialogPlayer != null && dialogPlayer.IsPlaying)
            dialogPlayer.StopCurrentScene();
        ClearQueue();
        currentQueuedScene = null;
    }

    public void SkipCurrentDialog()
    {
        dialogPlayer.StopCurrentNodeLine();
    }

    [Serializable]
    private class QueuedScene
    {
        public DialogScene Scene;
        public int Priority;
        public QueuedScene(DialogScene scene, int priority)
        {
            Scene = scene;
            Priority = priority;
        }
    }
}