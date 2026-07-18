using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour
{
    [SerializeField] private InterfaceComand interfaceComand = InterfaceComand.None;

    public InterfaceComand InterfaceComand => interfaceComand;
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() => InterfaceManager.ClickButton(interfaceComand));
    }
}
