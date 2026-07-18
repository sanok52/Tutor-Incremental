using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMainPlayer : Singleton<GameMainPlayer>
{
    private List<IGameStep> loop = new List<IGameStep>();
    private PlayGameLoopContext context;
    private Dictionary<Type, List<object>> playObjectsByStepType = new Dictionary<Type, List<object>>();
    private Dictionary<Type, List<object>> playObjectsRoutineByStepType = new Dictionary<Type, List<object>>();

    public override bool IsDontDestroyOnLoad => false;

    public void Init()
    {
        // Находим все типы, реализующие IGameStep (не интерфейсы и не абстрактные)
        var stepTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(t => typeof(IGameStep).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        loop = new List<IGameStep>();
        foreach (var type in stepTypes)
        {
            // Создаём экземпляр (требуется конструктор без параметров)
            var step = (IGameStep)Activator.CreateInstance(type);
            loop.Add(step);
        }

        // Сортируем по ExecutionOrder
        loop = loop.OrderBy(step => step.ExecutionOrder).ToList();

        if (loop.Count > 0)
            StartLoop(loop);
    }

    private void StartLoop(List<IGameStep> loop)
    {
        context = new PlayGameLoopContext();
        StartCoroutine(GameLoop(loop.ToArray()));
    }

    private IEnumerator GameLoop(IGameStep[] loopSteps)
    {
        while (true)
        {
            foreach (var step in loopSteps)
            {
                context.CurrentGameStep = step;
                Debug.Log($"[{step.ID}]");
                yield return step.Execute(context);
                yield return null;
            }
        }
    }

    public void Register<T>(IPlayGameObject<T> obj) where T : GameStepBase<T>
    {
        var type = typeof(T);
        if (!playObjectsByStepType.ContainsKey(type))
            playObjectsByStepType[type] = new List<object>();

        if (!playObjectsByStepType[type].Contains(obj))
        {
            playObjectsByStepType[type].Add(obj);
            playObjectsByStepType[type] = playObjectsByStepType[type]
                .OrderBy(o => ((IPlayGameObject<T>)o).Order).ToList();
        }
    }

    public void Register<T>(IPlayGameObjectRoutine<T> obj) where T : GameStepBase<T>
    {
        var type = typeof(T);
        if (!playObjectsRoutineByStepType.ContainsKey(type))
            playObjectsRoutineByStepType[type] = new List<object>();

        if (!playObjectsRoutineByStepType[type].Contains(obj))
        {
            playObjectsRoutineByStepType[type].Add(obj);
            // Теперь сортируем по правильному словарю
            playObjectsRoutineByStepType[type] = playObjectsRoutineByStepType[type]
                .OrderBy(o => ((IPlayGameObjectRoutine<T>)o).Order).ToList();
        }
    }

    public void Unregister<T>(IPlayGameObject<T> obj) where T : GameStepBase<T>
    {
        var type = typeof(T);
        if (playObjectsByStepType.TryGetValue(type, out var list))
            list.Remove(obj);
    }

    public void Unregister<T>(IPlayGameObjectRoutine<T> obj) where T : GameStepBase<T>
    {
        var type = typeof(T);
        if (playObjectsRoutineByStepType.TryGetValue(type, out var list))
            list.Remove(obj);
    }

    public List<IPlayGameObject<T>> GetObjectsForStep<T>() where T : GameStepBase<T>
    {
        var type = typeof(T);
        if (playObjectsByStepType.TryGetValue(type, out var list))
            return list.Cast<IPlayGameObject<T>>().ToList();
        return new List<IPlayGameObject<T>>();
    }

    public List<IPlayGameObjectRoutine<T>> GetObjectsForStepRoutine<T>() where T : GameStepBase<T>
    {
        var type = typeof(T);
        if (playObjectsRoutineByStepType.TryGetValue(type, out var list))
            return list.Cast<IPlayGameObjectRoutine<T>>().ToList();
        return new List<IPlayGameObjectRoutine<T>>();
    }
}