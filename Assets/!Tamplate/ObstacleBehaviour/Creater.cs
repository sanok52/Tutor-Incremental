using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creater : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private GameObject[] prefs;

    [SerializeField] private Vector2 scaleCofMinMax = Vector2.one;
    [SerializeField] private bool isChild;

    public void Create()
    {
        GameObject ngo = Instantiate(prefs[Random.Range(0, prefs.Length)], point.position, point.rotation);
        ngo.transform.localScale *= Random.Range(scaleCofMinMax.x, scaleCofMinMax.y);

        if (isChild)
            ngo.transform.parent = transform;
    }
}
