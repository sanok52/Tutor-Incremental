using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Debug/Test Booleans Config", fileName = "TestBooleansConfig")]
public class TestBooleansConfig : ScriptableObject
{
    public bool IsDebug = true;
    public List<TestBool> testBools = new List<TestBool>();
}