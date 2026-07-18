using System.Collections;

public interface IPlayGameObject<T> where T : GameStepBase<T>
{
    int Order { get; }
    bool IsEndPlay => true;
    bool IsEnable { get; }

    void OnStateStart(PlayGameLoopContext context);
    void OnStateUpdate(PlayGameLoopContext context);
    void OnStateEnd(PlayGameLoopContext context);
}

public interface IPlayGameObjectRoutine<T> where T : GameStepBase<T>
{
    int Order { get; }
    bool IsEndPlay => true;
    bool IsEnable { get; }

    IEnumerator OnStateUpdate(PlayGameLoopContext context);
}