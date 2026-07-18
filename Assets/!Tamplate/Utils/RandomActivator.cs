using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomActivator : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>();
    public bool playOnStart;

    private void Start()
    {
        if (playOnStart)
            Activate();
    }

    private void Activate()
    {
        gameObjects.RemoveAll(x => x == null);
        foreach (var item in gameObjects)
        {
            item.SetActive(false);
        }
        gameObjects.ToArray().RandomElement().SetActive(true);
    }
}
