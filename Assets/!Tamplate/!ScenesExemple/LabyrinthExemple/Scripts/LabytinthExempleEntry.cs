using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class LabytinthExempleEntry : MonoBehaviour
{
    [SerializeField] private LabyrinthModel model;
    [SerializeField] private LabyrinthDataSO dataSO;

    [Space]
    [SerializeField] private LabyrinthView[] labyrinthViews;
    [SerializeField] private MonoLabyrTP[] viewsPlayer;
    [SerializeField] private RoadViewMono[] roadViews;

    [Space]
    [SerializeField] private MonoLabyrTP player;
    [SerializeField] private LabytinthExPlayerController controller;
    [SerializeField] private LabyrintGameFlow gameFlow;

    [Space]
    private SceneExecuterContext globalContext = new SceneExecuterContext();

    public LabyrinthData Data { get; private set; }

    private void Awake()
    {
        Data = dataSO.Data;
        model.Init(dataSO.Data);
        player.LabyrTransform.Init(model.Data, model.Data.GetEnterNode().ID, Vector2Int.down);

        foreach (var view in labyrinthViews)
        {
            view.Init(model);
        }

        foreach (var view in viewsPlayer)
        {
            (view as ILabyrintTransformPresenter).SetTransform(player.LabyrTransform);
            view.UpdatePositionWork(player.LabyrTransform.GetPosition());
            view.UpdateDirectionWork(player.LabyrTransform.GetDirection());
        }

        foreach (var view in roadViews)
        {
            model.OnRoomViewUpdate += view.View;
        }

        globalContext.LabyrinthModel = model;
        globalContext.PlayerLTransfom = player.LabyrTransform;

        controller.Init(player.LabyrTransform);

        SceneExecuter.Instance.SetGlobalExecuterContext(globalContext);

        gameFlow.StartGameFlow(player.LabyrTransform, model, controller);
    }
}