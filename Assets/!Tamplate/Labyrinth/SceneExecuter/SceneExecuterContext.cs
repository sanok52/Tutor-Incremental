using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SceneExecuterContext : EnumCommandContext<SceneCtxVar>
{
    public LabyrinthObjectTransform PlayerLTransfom;
    public LabyrinthModel LabyrinthModel;

    [Space]
    public LabyrinthNode NodeInvoker;
    public LabyrinthData Labyrinth;

    [Space, Header("GameFlow")]
    public bool BreakToEndBehaviour;
    internal object NodePrevious;

    public SceneExecuterContext()
    {

    }

    public SceneExecuterContext(SceneExecuterContext globalContext)
    {
        PlayerLTransfom = globalContext.PlayerLTransfom;
        LabyrinthModel = globalContext.LabyrinthModel;
    }

    /*public string GetRoomText()
    {
        LabyrinthNode NodeInvoker = Get<LabyrinthNode>(SceneCtxVar.NodeInvoker);
        string Description = Get<string>(SceneCtxVar.Description);

        if (NodeInvoker.isEnterNode)
            return "There is a door in one of the walls of this room. \nJudging by the light coming through its cracks, it leads <color=#FF00C1>outside.</color>";
        
        if (!string.IsNullOrEmpty(Description)) //Полученное из других поведений
                return Description;

        if (!string.IsNullOrEmpty(NodeInvoker.Description)) //Полученное из данных узла
            return NodeInvoker.Description;

        return Get<string[]>(SceneCtxVar.EmptyRoomTexts).RandomElement();
    }*/

    public class MemorobleChoices
    {
        public LNodeBehCommand commandExecuter;
        public string commandTag;
        public string[] ChoiceTexts;
        public string AdditionDescroption;
    }
}

public enum SceneCtxVar
{
    None
}