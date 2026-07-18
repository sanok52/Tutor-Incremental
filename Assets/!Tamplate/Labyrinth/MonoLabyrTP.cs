using UnityEngine;

public class MonoLabyrTP : MonoBehaviour, ILabyrintTransformPresenter
{
    private LabyrinthObjectTransform labyrTransform;
    public LabyrinthObjectTransform LabyrTransform { get
        {
            if(labyrTransform == null)            
                labyrTransform = new LabyrinthObjectTransform();
            return labyrTransform;
        }
    }

    [SerializeField] private bool isSubscrabeOnStart = true;

    private void Start()
    {
        if (!isSubscrabeOnStart)
            return;

        (this as ILabyrintTransformPresenter).SetTransform(LabyrTransform);
    }

    public void SetTransformNotSubscrube(LabyrinthObjectTransform transform)
    {
        labyrTransform = transform;
    }

    public virtual void UpdateDirectionWork(Vector2Int newDirection)
    {
    }

    public virtual void UpdatePositionWork(Vector2Int newPosition)
    {
    }

    public void OnDestroy()
    {
       (this as ILabyrintTransformPresenter).ClearSubscription();
    }
}
