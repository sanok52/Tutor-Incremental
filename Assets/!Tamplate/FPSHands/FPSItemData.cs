using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "FPS Hands/Item Data")]
public class FPSItemData : ScriptableObject
{
    public string ID;

    [Space]
    [Header("Свойства")]
    public bool isLarge;                 // занимает обе руки
    public bool canDrop = true;
    public bool canStow = true;
    public bool canUse = true;
    public bool canUseHold = false;
    public bool canUseInteraction = true;
    public bool canStack = false;
    public int maxCountInHand = 1;

    [Space]
    public InfoElementContainer[] surfaces;   // поверхности, на которые можно класть предмет

    [Space]
    public HandsItemBehContainer behContainer;
    public KeyCode OverrideUseButton = KeyCode.None;

    [Space]
    public string[] tags;
}