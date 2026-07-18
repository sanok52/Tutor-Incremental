using System.Collections;
using UnityEngine;

public class LabyrintGameFlow : MonoBehaviour
{
    private LabyrinthModel model;
    private LabyrinthObjectTransform player;
    private LabytinthExPlayerController controller;

    private LabyrinthNode previousNode;

    public void StartGameFlow (LabyrinthObjectTransform player, 
        LabyrinthModel model, 
        LabytinthExPlayerController controller)
    {
        this.model = model;
        this.player = player;
        this.controller = controller;

        StartCoroutine(GameFlow());
    }

    public IEnumerator GameFlow()
    {
        bool isPlayerMove = true;
        player.OnPositionUpdate += (v2) => isPlayerMove = true;

        while (true)
        {
            while (!isPlayerMove)
            {
                yield return null;
            }

            isPlayerMove = false;
            yield return CurrentNodeBehaviour();
        }
    }

    private IEnumerator CurrentNodeBehaviour()
    {
        controller.SetBlock(true);

        var node = model.Data.GetNode(player.NodeID);
        LNodeBehContainer[] behaviourContainers = node.GetBehaviourContainers();
        yield return SceneExecuter.Instance.ExecuteBehaviours(behaviourContainers, GetContext(node));

        previousNode = node;
        controller.SetBlock(false);
    }

    private SceneExecuterContext GetContext(LabyrinthNode node)
    {
        return node.ApplyDataToContext(new SceneExecuterContext()
        {
            NodeInvoker = node,
            NodePrevious = previousNode,
            Labyrinth = model.Data,
            PlayerLTransfom = player
            //memory = LabyrintSceneMemory.GetNodeMemory(node)
        });
    }
}
