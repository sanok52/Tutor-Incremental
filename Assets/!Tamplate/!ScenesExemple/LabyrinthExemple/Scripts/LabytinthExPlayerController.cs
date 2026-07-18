using System;
using UnityEngine;

public class LabytinthExPlayerController : MonoBehaviour
{
    private LabyrinthObjectTransform transformL;

    public LabyrinthObjectTransform TransformL => transformL;

    [SerializeField] private bool isRotateMove;
    private bool isBlock;

    public void Init(LabyrinthObjectTransform transformL)
    {
        this.transformL = transformL;
    }

    void Update()
    {
        if (isBlock)
            return;

        if (Input.GetKeyUp(KeyCode.W))
            transformL.Translate(MoveDirection.Fwd, isRotateMove);
        else if (Input.GetKeyUp(KeyCode.S))
            transformL.Translate(MoveDirection.Back, isRotateMove);
        else if (Input.GetKeyUp(KeyCode.D))
            transformL.Translate(MoveDirection.Right, isRotateMove);
        else if (Input.GetKeyUp(KeyCode.A))
            transformL.Translate(MoveDirection.Left, isRotateMove);

        if(Input.GetKeyUp(KeyCode.E))
            transformL.Rotate(MoveDirection.Right);
        else if (Input.GetKeyUp(KeyCode.Q))
            transformL.Rotate(MoveDirection.Left);
    }

    public void SetBlock(bool isBlock)
    {
        this.isBlock = isBlock;
    }
}
