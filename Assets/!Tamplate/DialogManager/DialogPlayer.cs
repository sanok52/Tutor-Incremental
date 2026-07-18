using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DialogPlayer : MonoBehaviour
{
    [SerializeField] private PlayerChoicePresenter choicePresenter;

    public DialogScene dialogSceneTest;

    private List<IOutSpikerView> outSpikerViews = new List<IOutSpikerView>();
    private Coroutine currentSceneRoutine;
    private bool isPlaying;
    private DialogScene currentPlayingScene;

    private bool isPlayingNode;
    private bool skipRequested;
    private Coroutine waiterCoroutine;

    public bool IsPlaying => isPlaying;
    public DialogScene CurrentPlayingScene => currentPlayingScene;

    public event Action<DialogScene> OnDialogStarted;
    public event Action<DialogScene> OnDialogEnded;
    public event Action<DialogNode> OnNodeStarted;
    public event Action<DialogNode> OnNodeFinished;

    private void Start()
    {
        if (dialogSceneTest != null)
            DOVirtual.DelayedCall(Time.deltaTime * 5, () => PlayScene(dialogSceneTest));
    }

    public void AddSpiker(IOutSpikerView outSpikerView)
    {
        if (!outSpikerViews.Contains(outSpikerView))
            outSpikerViews.Add(outSpikerView);
    }

    public void RemoveSpiker(IOutSpikerView outSpikerView)
    {
        outSpikerViews.Remove(outSpikerView);
    }

    public void PlayScene(DialogScene scene)
    {
        if (currentSceneRoutine != null)
            StopCurrentScene();
        currentPlayingScene = scene;
        currentSceneRoutine = StartCoroutine(PlaySceneRoutine(scene));
    }

    private bool stopProcess = false;
    public void StopCurrentScene()
    {
        stopProcess = true;

        if (currentSceneRoutine != null)
        {
            StopCoroutine(currentSceneRoutine);
            currentSceneRoutine = null;
        }
        BreakCurrentScene();
        if (currentPlayingScene != null)
        {
            var scene = currentPlayingScene;
            currentPlayingScene = null;
            OnDialogEnded?.Invoke(scene);
        }

        if (waiterCoroutine != null)
            StopCoroutine(waiterCoroutine);

        foreach (var coroutine in coroutinesSpeek)        
            StopCoroutine(coroutine);
        
        isPlaying = false;
        stopProcess = false;
    }

    private List<Coroutine> coroutinesSpeek = new List<Coroutine>();
    private IEnumerator PlaySceneRoutine(DialogScene scene)
    {
        while (stopProcess)
            yield return new WaitForEndOfFrame();

        isPlaying = true;
        OnDialogStarted?.Invoke(scene);
        coroutinesSpeek.Clear();

        string currentNodeId = scene.startNodeID;
        int safetyCounter = 0;
        const int maxSteps = 1000;

        while (isPlaying && !string.IsNullOrEmpty(currentNodeId) && safetyCounter++ < maxSteps)
        {
            DialogNode node = scene.dialogScenes.Find(n => n.NodeID == currentNodeId);
            if (node == null)
            {
                Debug.LogError($"Node with ID {currentNodeId} not found in scene {scene.name}");
                break;
            }

            OnNodeStarted?.Invoke(node);
            if (node.actions != null)
                InvokeActions(node.actions.ToArray());

            if (node.nodeType != DialogNodeType.Empty && node.nodeType != DialogNodeType.Exit)
            {
                skipRequested = false;

                OutDialogElement element = new OutDialogElement { IDSpiker = node.SpikerID, Text = node.Text };

                coroutinesSpeek = new List<Coroutine>();
                foreach (var spiker in outSpikerViews)
                {
                    if (spiker.IsPlay(element))
                        coroutinesSpeek.Add(StartCoroutine(spiker.Play(element)));
                }
                waiterCoroutine = StartCoroutine(WaiterRoutine(coroutinesSpeek.ToArray()));

                while (isPlayingNode && !skipRequested)
                    yield return null;
                if (skipRequested && waiterCoroutine != null)
                {
                    StopCoroutine(waiterCoroutine);
                    foreach (var coroutine in coroutinesSpeek)
                        StopCoroutine(coroutine);                    
                    foreach (var spiker in outSpikerViews)
                        spiker.Break();
                }
                waiterCoroutine = null;
            }

            OnNodeFinished?.Invoke(node);

            // --- Определение следующей ноды (без изменений) ---
            List<DialogTransition> outgoing = scene.transitions.FindAll(t => t.IDOut == currentNodeId);

            if (outgoing.Count == 0 || node.nodeType == DialogNodeType.Exit)
                break;

            List<DialogTransition> choices = outgoing.FindAll(t => t.dialogTransitions == DialogTransitionType.PlayerChoice);
            if (choices.Count > 0)
            {
                string[] choiceTexts = new string[choices.Count];
                for (int i = 0; i < choices.Count; i++)
                    choiceTexts[i] = choices[i].choiceText;

                int selectedIndex = -1;
                yield return WaitPlayerChoice(choiceTexts, idx => selectedIndex = idx);
                if (selectedIndex < 0 || selectedIndex >= choices.Count)
                    selectedIndex = 0;

                currentNodeId = choices[selectedIndex].IDIn;
                if (choices[selectedIndex].dialogAction != null)
                    InvokeActions(choices[selectedIndex].dialogAction.ToArray());
                continue;
            }

            string nextNodeId = null;
            foreach (var transition in outgoing)
            {
                if (transition.dialogTransitions == DialogTransitionType.ConditionTransition)
                {
                    if (DialogConditionMediator.TestCondition(transition.transitionCondition))
                    {
                        nextNodeId = transition.IDIn;
                        break;
                    }
                }
                else if (transition.dialogTransitions == DialogTransitionType.Empty)
                {
                    nextNodeId = transition.IDIn;
                }
            }
            currentNodeId = nextNodeId;
        }

        if (safetyCounter >= maxSteps)
            Debug.LogError($"Possible infinite loop in dialog scene {scene.name}");

        currentSceneRoutine = null;
        if (currentPlayingScene == scene)
            currentPlayingScene = null;

        Debug.Log("isPlaying = false");
        isPlaying = false;
        OnDialogEnded?.Invoke(scene);
    }

    private IEnumerator WaiterRoutine(Coroutine[] coroutines)
    {
        isPlayingNode = true;
        foreach (var cor in coroutines)
            yield return cor;
        isPlayingNode = false;
    }

    private void InvokeActions(DialogActionInfo[] actions)
    {
        foreach (var action in actions)
            DialogActionMediator.InvokeCommand(action);
    }

    public IEnumerator WaitPlayerChoice(string[] choices, Action<int> onChoice, float wait = 10f)
    {
        if (choicePresenter == null)
        {
            Debug.LogError("PlayerChoicePresenter not assigned");
            onChoice?.Invoke(0);
            yield break;
        }
        choicePresenter.gameObject.SetActive(true);
        yield return choicePresenter.WaitPlayerChoice(choices, onChoice, wait);
        choicePresenter.gameObject.SetActive(false);
    }

    public void BreakCurrentScene()
    {
        skipRequested = true;
        if (waiterCoroutine != null)
            StopCoroutine(waiterCoroutine);
        // Дополнительно останавливаем всё, на всякий случай
        foreach (var spiker in outSpikerViews)
            spiker.Break();
        StopChoice();
        if (currentSceneRoutine != null)
        {
            StopCoroutine(currentSceneRoutine);
            currentSceneRoutine = null;
        }
        isPlaying = false;
    }

    private void StopChoice()
    {
        if (choicePresenter != null)
        {
            choicePresenter.StopAllCoroutines();
            choicePresenter.gameObject.SetActive(false);
        }
    }

    public void StopCurrentNodeLine()
    {
        skipRequested = true;
    }
}

    [Serializable]
public class DialogTransition
{
    public string TransitionID;
    public string IDOut;
    public string IDIn;
    public DialogTransitionType dialogTransitions;
    public string choiceText; // текст для выбора игрока
    public DialogTransitionCondition transitionCondition;

    [Space]
    public List<DialogActionInfo> dialogAction = new List<DialogActionInfo>();
}

public enum DialogNodeType
{
    Empty,          // ничего не воспроизводит, сразу переход
    TextView,       // воспроизвести текст
    PlayerChoice,   // показать выбор игрока
    Exit            // завершить сцену без воспроизведения
}

public enum DialogTransitionType
{
    Empty,                // безусловный переход (fallback)
    ConditionTransition,  // переход по условию
    PlayerChoice          // вариант выбора игрока
}

[Serializable]
public class DialogTransitionCondition
{
    public string ValueName;      // имя переменной для проверки
    public string CompareCommand; // оператор сравнения (например ">", "==", "<=")
    public float ValueFl;         // значение для сравнения (если float)
    public string ValueStr;       // значение для сравнения (если string)
}

[Serializable]
public class DialogActionInfo
{
    public string CommandName;      // имя переменной
    public string ActionCommand; // оператор комманды
    public float ValueFl;         // значение для выполнения команды (float)
    public string ValueStr;       // значение для выполнения команды (string)
}