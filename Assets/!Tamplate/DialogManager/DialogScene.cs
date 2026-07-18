using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogScene", menuName = "ChessNock/DialogSystem/DialogScene")]
public class DialogScene : ScriptableObject
{
    public string startNodeID;
    public List<DialogNode> dialogScenes = new List<DialogNode>();
    public List<DialogTransition> transitions = new List<DialogTransition>();

    [Space]
    public int Priority = 1000;
    public bool BreakEqualPriority = true;
    public bool LostIsBreak = true;
    public bool LostIsNotPlay = false;
    public string[] killDialogScenes = new string[0];

    private void OnEnable()
    {
        if (dialogScenes == null) dialogScenes = new List<DialogNode>();
        if (transitions == null) transitions = new List<DialogTransition>();
    }
}