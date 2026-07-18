using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnumData", menuName = "Enum Editor/Data")]
public class EnumData : ScriptableObject
{
    public List<EnumDefinition> enums = new List<EnumDefinition>();
    public string folderPath = "Assets/Scripts/GeneratedEnums";
}

[System.Serializable]
public class EnumDefinition
{
    public string name;
    public List<string> values = new List<string>();
}