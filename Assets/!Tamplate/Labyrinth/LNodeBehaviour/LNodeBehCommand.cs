using System;
using System.Collections;
using System.Collections.Generic;

public abstract class LNodeBehCommand : CommandBehaviour<SceneExecuterContext>, ICmdBehaviourWithRoutine<SceneExecuterContext>
{
    public virtual bool IsBreakToEndBehaviour { get; } = true;

    public IEnumerator ExecuteRoutine(SceneExecuterContext context)
    {
        if (context.BreakToEndBehaviour && IsBreakToEndBehaviour)
            yield break;
    }

    public virtual IEnumerator ExecuteMemarbleCommand(SceneExecuterContext.MemorobleChoices memoroble, int choice, SceneExecuterContext context)
    {
        yield return null;
    }

    public virtual SceneExecuterContext.MemorobleChoices GetMemorobleChoices(string tag, params string[] choiceTexts)
    {
        return new SceneExecuterContext.MemorobleChoices
        {
            commandExecuter = this,
            commandTag = tag,
            ChoiceTexts = choiceTexts
        };
    }

    public virtual void ApplyDataToMemory(LabyrinthNodeMemoryElement memoryElement)
    {

    }
}