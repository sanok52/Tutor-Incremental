using System.Collections.Generic;
using UnityEngine;
using System.Collections;

// ===== Базовый интерфейс команды =====
public interface ICommandBehaviour<in C> where C : CommandContext
{
    void Execute(C context);
}

// ===== Абстрактная MonoBehaviour-команда =====
// Наследники автоматически получают Execute и могут переопределить его.
public abstract class CommandBehaviour<C> : MonoBehaviour, ICommandBehaviour<C> where C : CommandContext
{
    public abstract void Execute(C context);
}

public interface ICmdBehaviourWithRoutine<C> where C : CommandContext
{
    public abstract IEnumerator ExecuteRoutine(C context);
}

// ===== Базовый интерфейс контейнера =====
public interface ICommandContainer<C> where C : CommandContext
{
    List<CommandBehaviour<C>> Behaviours { get; }
    void ExecuteAll(C context);
    IEnumerator ExecuteAllRoutine(C context, bool waitEach = true);
}

// ===== Абстрактный MonoBehaviour-контейнер =====
public abstract class CommandContainer<C> : MonoBehaviour, ICommandContainer<C> where C : CommandContext
{
    [SerializeReference] // важно: храним ссылки на компоненты
    protected List<CommandBehaviour<C>> behaviours = new List<CommandBehaviour<C>>();

    public List<CommandBehaviour<C>> Behaviours => behaviours;

    public virtual void ExecuteAll(C context)
    {
        foreach (var cmd in behaviours)
            if (cmd != null) cmd.Execute(context);
    }

    public virtual IEnumerator ExecuteAllRoutine (C context, bool waitEach = true)
    {
        List<Coroutine> coroutines = new List<Coroutine>();
        foreach (var cmd in behaviours)
        {
            if (cmd != null)
            {
                ICmdBehaviourWithRoutine<C> withRoutine = cmd as ICmdBehaviourWithRoutine<C>;
                if (withRoutine == null)
                    continue;

                var routine = withRoutine.ExecuteRoutine(context);
                if (routine != null)
                {
                    var coroutine = StartCoroutine(routine);
                    coroutines.Add(coroutine);
                    if (waitEach) yield return coroutine;
                    if (context.BreakAllNextBehaviour)
                        break;
                }
            }
        }

        foreach (var routine in coroutines)
        {
            yield return routine;
        }
    }

    // Методы для редактирования списка (используются кастомным редактором)
    public void AddCommand(CommandBehaviour<C> cmd)
    {
        if (!behaviours.Contains(cmd))
            behaviours.Add(cmd);
    }

    public void RemoveCommand(CommandBehaviour<C> cmd)
    {
        behaviours.Remove(cmd);
    }
}