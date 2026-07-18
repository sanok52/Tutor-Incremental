using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum EventerLoopType
{
    None,       // Один раз
    Loop,       // Повторять по кругу
    PingPong    // Вперёд и назад
}

public class TimerEventer : MonoBehaviour
{
    [SerializeField] private TimerEventerEvent[] eventerEvents;
    [SerializeField] private EventerLoopType loopType = EventerLoopType.None;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool playOnEnable = true;

    private Coroutine routine;
    private int currentIndex = 0;
    private bool forward = true;

    private void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    private void Start()
    {
        if (playOnStart)
            Play();
    }

    public void Play()
    {
        Stop(); // На случай, если уже работает
        if (eventerEvents == null || eventerEvents.Length == 0)
            return;

        routine = StartCoroutine(PlayRoutine());
    }

    public void Stop()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    public void Restart()
    {
        currentIndex = 0;
        forward = true;
        Play();
    }

    private IEnumerator PlayRoutine()
    {
        while (true)
        {
            if (currentIndex < 0 || currentIndex >= eventerEvents.Length)
                yield break;

            var evt = eventerEvents[currentIndex];

            yield return new WaitForSeconds(evt.delay);
            yield return new WaitWhile(() => GamePause.IsPause);
            evt.OnInvoke?.Invoke();

            switch (loopType)
            {
                case EventerLoopType.None:
                    currentIndex++;
                    if (currentIndex >= eventerEvents.Length)
                        yield break;
                    break;

                case EventerLoopType.Loop:
                    currentIndex = (currentIndex + 1) % eventerEvents.Length;
                    break;

                case EventerLoopType.PingPong:
                    if (forward)
                    {
                        currentIndex++;
                        if (currentIndex >= eventerEvents.Length)
                        {
                            currentIndex = eventerEvents.Length - 2;
                            forward = false;
                        }
                    }
                    else
                    {
                        currentIndex--;
                        if (currentIndex < 0)
                        {
                            currentIndex = 1;
                            forward = true;
                        }
                    }
                    break;
            }
        }
    }
}

[System.Serializable]
public class TimerEventerEvent
{
    public string title;
    public UnityEvent OnInvoke;
    public float delay = 1f;
}