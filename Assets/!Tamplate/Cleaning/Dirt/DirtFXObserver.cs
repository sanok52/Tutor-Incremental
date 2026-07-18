using UnityEngine;

public class DirtFXObserver : MonoBehaviour
{
    [Header("Target Dirt")]
    [SerializeField] private Dirt dirt;                // если не назначен, найдётся сам на этом объекте

    [Header("Point Clean Effect")]
    [SerializeField] private GameObject pointCleanEffectPrefab;   // префаб эффекта (ParticleSystem и т.п.)
    [SerializeField] private float pointCleanStep = 0.2f;        // минимальный интервал между эффектами
    [SerializeField] private bool followPosition = false;         // если true, эффект будет привязан к точке контакта

    [Header("Complete Clean Effect")]
    [SerializeField] private GameObject completeCleanEffectPrefab; // эффект при полной очистке

    private float lastPointEffectTime;
    private bool completeTriggered = false;

    private void OnEnable()
    {
        if (dirt == null)
            dirt = GetComponent<Dirt>();

        if (dirt == null)
        {
            Debug.LogError("DirtFXObserver: не найден компонент Dirt");
            enabled = false;
            return;
        }

        dirt.OnCleaned += HandlePointClean;
        dirt.OnCleanProgress += HandleProgress;
    }

    private void OnDisable()
    {
        if (dirt != null)
        {
            dirt.OnCleaned -= HandlePointClean;
            dirt.OnCleanProgress -= HandleProgress;
        }
    }

    private void HandlePointClean(Dirt.CleanResultInfo info)
    {
        if (Time.time - lastPointEffectTime < pointCleanStep)
            return;

        lastPointEffectTime = Time.time;
        PlayEffect(pointCleanEffectPrefab, info.worldPosition, Quaternion.identity, followPosition);
    }

    private void HandleProgress(float percent)
    {
        // Считаем полностью очищенным, когда процент >= 99.9 (на случай погрешности)
        //Debug.Log($"HandleProgress {percent}");

        if (!completeTriggered && percent >= 95.9f)
        {
            completeTriggered = true;
            PlayEffect(completeCleanEffectPrefab, transform.position, Quaternion.identity, false);
            dirt.Clear();
        }
    }

    private void PlayEffect(GameObject prefab, Vector3 position, Quaternion rotation, bool attachToWorld)
    {
        if (prefab == null)
            return;

        if (attachToWorld)
        {
            // Спавним в мире (не привязываем к пятну)
            Instantiate(prefab, position, rotation);
        }
        else
        {
            // Спавним как дочерний объект Dirt, чтобы двигался вместе с ним
            Instantiate(prefab, position, rotation, transform);
        }
    }
}