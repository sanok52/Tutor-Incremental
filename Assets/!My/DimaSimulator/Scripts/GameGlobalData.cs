using System.Collections.Generic;
using UnityEngine;

public class GameGlobalData : MonoBehaviour
{
    public List<Author> allAuthors;          // все возможные авторы (шаблоны)
    public List<ReactionTypeInfo> allReactionTypes; // все возможные типы реакций с характеристиками

    [Header("Ресурсы")]
    public List<IntContainer> PlayerResourcesInit = new List<IntContainer>();
    public List<ReactionSetting> availableReactions = new List<ReactionSetting>();
    public PocketRandomDataCreate<int>[] RandomRangOffset;
}