using System.Collections;
using UnityEngine;

public class ContainerRunner : MonoBehaviour
{
    [SerializeField] private ExempeCommandContainer cmdContainer;
    [SerializeField] private Vector3 hitPoint = Vector3.one;
    [SerializeField] private bool waitEach = false;

    private void Start()
    {
        Debug.Log($"In ExempeCommandContainer {cmdContainer.Behaviours.Count} commands");
        cmdContainer = Instantiate(cmdContainer, transform);
        cmdContainer.ExecuteAll(new ExempleCmdContext() { hitPoint = hitPoint });
        StartCoroutine(ExAllRoutune());
    }

    public IEnumerator ExAllRoutune()
    {
        yield return cmdContainer.ExecuteAllRoutine(new ExempleCmdContext() { hitPoint = hitPoint },
            waitEach);
        Debug.Log("All routines completed");
    }
}
