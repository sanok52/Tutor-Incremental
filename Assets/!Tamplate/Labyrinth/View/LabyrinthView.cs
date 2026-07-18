using System;
using UnityEngine;

public class LabyrinthView : MonoBehaviour
{
    [SerializeField] private LabyrinthData data = null;
    public virtual LabyrinthData Data => labyrinthModel != null ? labyrinthModel.Data : data;

    [SerializeField] private bool isUpdateDataEveryFrame = true;

    private LabyrinthModel labyrinthModel;


    public virtual void Update()
    {
        if (!isUpdateDataEveryFrame)
            return;

        UpdateView(Data);
    }

    public void Init(LabyrinthModel model)
    {
        labyrinthModel = model;
        UpdateView(data);
    }

    public virtual void UpdateView(LabyrinthData data)
    {
        this.data = data;
    }

    public virtual Vector3 GetWorldPosition(Vector2Int position)
    {
        return transform.position + new Vector3(position.x, -position.y);
    }
}