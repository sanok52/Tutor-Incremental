using UnityEngine;
using DG.Tweening;

public class PositionFromViewerLabyrLabyrTP : MonoLabyrTP
{
    [SerializeField] private Transform viewTransform;
    [SerializeField] private bool isRotateMove = true;
    public bool IsRotateMove => isRotateMove;
    [SerializeField] private Vector3 vectorUp = Vector3.forward;
    [SerializeField] private bool flat = true;

    [SerializeField]
    private DOTweenPositionData moveData = new DOTweenPositionData()
    {
        IsLocal = false,
        Duration = 0.5f,
        Ease = Ease.OutQuad,
        isKillAndCompleteLastTweener = true,
        isKillLastTweener = true,
        useX = true,
        useY = true,
        usez = true,
    };

    [SerializeField] 
    private DOTweenRotationData rotateData = new DOTweenRotationData()
    {
        Duration = 0.5f,
        Ease = Ease.OutQuad,
        isKillAndCompleteLastTweener = true,
        isKillLastTweener = true,
        RotateMode = RotateMode.Fast
    };

    [Space]
    [SerializeField] private LabyrinthView labyrinthView;
    [SerializeField] private bool IsPlayInEditor = true;

    public LabyrinthView LabyrinthView => labyrinthView;

    public Transform ViewTransform
    {
        get
        {
            if (viewTransform == null)
                viewTransform = transform;
            return viewTransform;
        }
    }

    private void OnValidate()
    {
        if(Application.isPlaying == false && LabyrTransform != null)
        {
            if (string.IsNullOrEmpty(LabyrTransform.NodeID) || LabyrTransform.Data == null)
                return;

            //UpdatePositionWork(LabyrTransform.GetPosition());
            //UpdateDirectionWork(LabyrTransform.GetDirection());
        }    
    }

    public override void UpdatePositionWork(Vector2Int newPosition)
    {
        if (labyrinthView == null)
            return;

        if (Application.isPlaying == false)
        {
            ViewTransform.position = labyrinthView.GetWorldPosition(newPosition);
            return;
        }
        
        moveData.TransformMove(ViewTransform, labyrinthView.GetWorldPosition(newPosition));
    }

    public override void UpdateDirectionWork(Vector2Int newDirection)
    {
        if (labyrinthView == null)
            return;

        Vector3 targetLook = Quaternion.LookRotation(new Vector3(newDirection.x, flat ? -newDirection.y : 0, flat == false ? -newDirection.y : 0), vectorUp).eulerAngles;

        if (Application.isPlaying == false)
        {
            ViewTransform.rotation = Quaternion.Euler(targetLook);
            return;
        }

        rotateData.TransformRotate(ViewTransform, targetLook);
    }
}