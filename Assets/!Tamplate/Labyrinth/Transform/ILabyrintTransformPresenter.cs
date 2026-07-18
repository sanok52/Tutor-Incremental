using UnityEngine;

public interface ILabyrintTransformPresenter
{
    public LabyrinthObjectTransform LabyrTransform { get; }

    public void Init(LabyrinthObjectTransform transform)
    {
        SetTransform(transform);
        UpdatePositionWork(transform.GetPosition());
        UpdateDirectionWork(transform.GetDirection());
    }

    public virtual void SetTransform(LabyrinthObjectTransform transform)
    {
        ClearSubscription();

        SetTransformNotSubscrube(transform);
        LabyrTransform.OnPositionUpdate += UpdatePositionWork;
        LabyrTransform.OnDirectionUpdate += UpdateDirectionWork;
    }

    public abstract void SetTransformNotSubscrube(LabyrinthObjectTransform transform);
    public abstract void UpdatePositionWork(Vector2Int newPosition);
    public abstract void UpdateDirectionWork(Vector2Int newDirection);

    public virtual void ClearSubscription()
    {
        if(LabyrTransform == null)
            return;

        LabyrTransform.OnPositionUpdate -= UpdatePositionWork;
        LabyrTransform.OnDirectionUpdate -= UpdateDirectionWork;
    }
}