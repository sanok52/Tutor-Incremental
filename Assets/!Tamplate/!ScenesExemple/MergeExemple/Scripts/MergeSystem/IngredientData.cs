using System;
using UnityEngine;

[Serializable]
public class IngredientData<E> where E : Enum
{
    public string ID;
    public E Type;
    public string Title;
    public Sprite Icon;
    public string Description;
}