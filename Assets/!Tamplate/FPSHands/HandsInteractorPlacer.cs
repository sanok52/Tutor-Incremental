using UnityEngine;

public class HandsInteractorPlacer : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _rotationSpeed = 90f;

    private Vector3 _currentNormal;
    private float _currentAngle;

    public bool IsShown => _target.gameObject.activeSelf;
    public Transform Target => _target;

    private void Update()
    {
        if (!IsShown) return;

        float scroll = Input.mouseScrollDelta.y;
        if (!Mathf.Approximately(scroll, 0f))
        {
            _currentAngle += scroll * _rotationSpeed * Time.deltaTime;
            ApplyRotation();
        }
    }

    public void Show(Vector3 position, Vector3 normal)
    {
        _target.position = position;
        _currentNormal = normal;
        _target.rotation = Quaternion.FromToRotation(Vector3.up, normal);
        ApplyRotation();
        _target.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _target.gameObject.SetActive(false);
    }

    public (Vector3 position, Quaternion rotation) GetPlacementInfo()
    {
        return (_target.position, _target.rotation);
    }

    private void ApplyRotation()
    {
        Quaternion baseRotation = Quaternion.FromToRotation(Vector3.up, _currentNormal);
        _target.rotation = baseRotation * Quaternion.Euler(0f, _currentAngle * Mathf.Rad2Deg, 0f);
    }
}