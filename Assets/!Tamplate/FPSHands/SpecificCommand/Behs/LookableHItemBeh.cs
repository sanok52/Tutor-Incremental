public class LookableHItemBeh : HandsItemBeh, IHandsItemUsed
{
    public void ExecuteEnter(HandsItemBehContext context)
    {
        if (context.ItemAnimator != null)
            context.ItemAnimator.StartUse();
    }

    public void ExecuteExit(HandsItemBehContext context)
    {
        if (context.ItemAnimator != null)
            context.ItemAnimator.EndUse();
    }

    public void ExecuteItem(HandsItemBehContext context)
    {
        if (context.ItemAnimator != null)
            context.ItemAnimator.SwithUse();
    }

    public void ExecuteUpdate(HandsItemBehContext context)
    {
    }
}
