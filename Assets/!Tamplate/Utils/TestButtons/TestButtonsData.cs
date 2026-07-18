using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TestButtonsData", menuName = "Test/Buttons Data")]
public class TestButtonsData : ScriptableObject
{
    // —писок данных каждой кнопки
    public List<ButtonEntry> entries = new();

    [System.Serializable]
    public class ButtonEntry
    {
        public string id;
        public List<string> strData = new();
        public List<GameObject> goData = new(); // сохран€ютс€ ссылки на объекты сцены (при перезагрузке сцены могут потер€тьс€, но дл€ редактора ок)
    }
}