using System.Collections;
using UnityEngine;
// Конкретная команда
public class SpecificExempleCmdBeh : ExempleCmdBeh, ICmdBehaviourWithRoutine<ExempleCmdContext>
{
    public float duration = 5f;
    public override void Execute(ExempleCmdContext ctx)
    {
        Debug.Log($"SpecificExempleCmdBeh Execute");
    }

    public IEnumerator ExecuteRoutine(ExempleCmdContext ctx)
    {
        Debug.Log($"SpecificExempleCmdBeh {duration} start");
        yield return new WaitForSeconds(duration);
        Debug.Log($"SpecificExempleCmdBeh {duration} end {ctx.hitPoint}");
    }
}