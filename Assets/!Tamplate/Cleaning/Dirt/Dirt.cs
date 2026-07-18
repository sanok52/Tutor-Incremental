using UnityEngine;
using System;

public class Dirt : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Texture2D dirtTex;
    [SerializeField] private Texture2D cleanTex;  // если null, чистые участки становятся прозрачными

    [Header("Mask Resolution")]
    [SerializeField] private int maskResolution = 128;

    [Header("Clean Falloff (для обычной круглой очистки)")]
    [SerializeField] private AnimationCurve falloffCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Instriments")]
    public InfoElementContainer Instruments;

    // События
    public event Action<float> OnCleanProgress;
    public event Action<CleanResultInfo> OnCleaned;

    private Texture2D maskTex;
    private Material stainMaterial;
    private float[,] dirtiness;          // текущая степень грязи (0 = чисто, 1 = полностью грязно)
    private float[,] initialDirtiness;   // исходная карта грязи (по альфе dirtTex)
    private float initialTotalDirt;
    private float totalDirtLeft;
    private float lastReportedPercent;

    private Bounds meshBounds;
    private float worldWidth, worldHeight;

    public struct CleanResultInfo
    {
        public Dirt dirt;
        public Vector3 worldPosition;
        public float radius;
        public float power;
        public float dirtRemoved;
        public float totalCleanPercent;
    }

    private void Awake()
    {
        var renderer = GetComponent<Renderer>();
        stainMaterial = renderer.material;

        if (cleanTex == null)
        {
            cleanTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            cleanTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
            cleanTex.Apply();
        }
        stainMaterial.SetTexture("_DirtTex", dirtTex);
        stainMaterial.SetTexture("_CleanTex", cleanTex);

        var meshFilter = GetComponent<MeshFilter>();
        meshBounds = meshFilter.mesh.bounds;
        worldWidth = meshBounds.size.x * transform.lossyScale.x;
        worldHeight = meshBounds.size.y * transform.lossyScale.y;

        maskTex = new Texture2D(maskResolution, maskResolution, TextureFormat.R8, false);
        maskTex.filterMode = FilterMode.Bilinear;
        maskTex.wrapMode = TextureWrapMode.Clamp;
        stainMaterial.SetTexture("_MaskTex", maskTex);

        dirtiness = new float[maskResolution, maskResolution];
        initialDirtiness = new float[maskResolution, maskResolution];
        initialTotalDirt = 0f;

        if (dirtTex != null && dirtTex.isReadable)
        {
            for (int y = 0; y < maskResolution; y++)
            {
                for (int x = 0; x < maskResolution; x++)
                {
                    float u = (x + 0.5f) / maskResolution;
                    float v = (y + 0.5f) / maskResolution;
                    float alpha = dirtTex.GetPixelBilinear(u, v).a;
                    initialDirtiness[x, y] = Mathf.Clamp01(alpha);
                    dirtiness[x, y] = initialDirtiness[x, y];
                    initialTotalDirt += initialDirtiness[x, y];
                }
            }
        }
        else
        {
            if (dirtTex != null && !dirtTex.isReadable)
                Debug.LogWarning("Dirt: dirtTex должна иметь Read/Write Enabled.");

            for (int y = 0; y < maskResolution; y++)
                for (int x = 0; x < maskResolution; x++)
                    dirtiness[x, y] = 1f;

            initialDirtiness = (float[,])dirtiness.Clone();
            initialTotalDirt = maskResolution * maskResolution;
        }

        totalDirtLeft = initialTotalDirt;
        UpdateFullMask();
        lastReportedPercent = 0f;
    }

    // --- Базовые перегрузки Clean (без маски кисти) ---

    public void Clean(Vector3 worldPos, float radius)
    {
        Clean(worldPos, radius, 1f, null);
    }

    public void Clean(Vector3 worldPos, float radius, float power)
    {
        Clean(worldPos, radius, power, null);
    }

    /// <summary>
    /// Главный метод очистки. Поддерживает круглую очистку с falloff-кривой или
    /// очистку произвольной формы через Texture2D‑маску, повёрнутую на rotation градусов.
    /// </summary>
    /// <param name="worldPos">Точка касания в мировых координатах</param>
    /// <param name="radius">Размер кисти (радиус в мировых единицах, охватывает квадрат 2×radius)</param>
    /// <param name="power">Общая сила очистки (0..1)</param>
    /// <param name="brushMask">Текстура кисти (чёрный – не очищает, белый – очищает полностью). Может быть null.</param>
    /// <param name="rotation">Поворот кисти вокруг точки касания в градусах (0 – без поворота)</param>
    public void Clean(Vector3 worldPos, float radius, float power, Texture2D brushMask, float rotation = 0f)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        float u = Mathf.InverseLerp(meshBounds.min.x, meshBounds.max.x, localPos.x);
        float v = Mathf.InverseLerp(meshBounds.min.y, meshBounds.max.y, localPos.y);

        if (u < 0f || u > 1f || v < 0f || v > 1f)
            return;

        int centerX = Mathf.RoundToInt(u * (maskResolution - 1));
        int centerY = Mathf.RoundToInt(v * (maskResolution - 1));

        float avgWorldSize = Mathf.Max(worldWidth, worldHeight);
        float uvRadius = radius / avgWorldSize;
        int pixelRadius = Mathf.CeilToInt(uvRadius * maskResolution);

        // Если маска не задана – используем старую круговую логику с falloff
        if (brushMask == null)
        {
            CleanCircularFalloff(worldPos, radius, power, centerX, centerY, pixelRadius);
            return;
        }

        if (!brushMask.isReadable)
        {
            Debug.LogWarning("Dirt: brushMask должна иметь Read/Write Enabled. Используется круговая замена.");
            CleanCircularFalloff(worldPos, radius, power, centerX, centerY, pixelRadius);
            return;
        }

        // Готовим параметры для произвольной маски
        float cosRot = Mathf.Cos(rotation * Mathf.Deg2Rad);
        float sinRot = Mathf.Sin(rotation * Mathf.Deg2Rad);
        float invRadius = 1f / (2f * radius); // для маппинга координат в UV [0,1]

        int minX = Mathf.Max(0, centerX - pixelRadius);
        int maxX = Mathf.Min(maskResolution - 1, centerX + pixelRadius);
        int minY = Mathf.Max(0, centerY - pixelRadius);
        int maxY = Mathf.Min(maskResolution - 1, centerY + pixelRadius);

        bool changed = false;
        float dirtRemovedThisCall = 0f;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                // Локальные координаты ячейки
                float localCellX = Mathf.Lerp(meshBounds.min.x, meshBounds.max.x, (x + 0.5f) / maskResolution);
                float localCellY = Mathf.Lerp(meshBounds.min.y, meshBounds.max.y, (y + 0.5f) / maskResolution);
                float dx = localCellX - localPos.x;
                float dy = localCellY - localPos.y;

                // Поворот смещения для сэмплирования текстуры
                float sampleU = (dx * cosRot - dy * sinRot) * invRadius + 0.5f;
                float sampleV = (dx * sinRot + dy * cosRot) * invRadius + 0.5f;

                if (sampleU < 0f || sampleU > 1f || sampleV < 0f || sampleV > 1f)
                    continue;

                float brushValue = brushMask.GetPixelBilinear(sampleU, sampleV).r;   // можно брать .r, .a или grayscale
                float cleanAmount = Mathf.Clamp01(power * brushValue);

                if (cleanAmount <= 0f)
                    continue;

                float oldVal = dirtiness[x, y];
                if (oldVal > 0f)
                {
                    float newVal = Mathf.Max(0f, oldVal - cleanAmount);
                    dirtiness[x, y] = newVal;
                    float removed = oldVal - newVal;
                    dirtRemovedThisCall += removed;
                    totalDirtLeft -= removed;
                    changed = true;
                }
            }
        }

        if (!changed)
            return;

        UpdateMaskRegion(minX, minY, maxX, maxY);

        float percent = initialTotalDirt > 0f ? (1f - totalDirtLeft / initialTotalDirt) * 100f : 100f;
        if (Mathf.Abs(percent - lastReportedPercent) > 0.1f)
        {
            lastReportedPercent = percent;
            OnCleanProgress?.Invoke(percent);
        }

        OnCleaned?.Invoke(new CleanResultInfo
        {
            dirt = this,
            worldPosition = worldPos,
            radius = radius,
            power = power,
            dirtRemoved = dirtRemovedThisCall,
            totalCleanPercent = percent
        });
    }

    /// <summary>
    /// Круговая очистка с falloff-кривой (старое поведение).
    /// </summary>
    private void CleanCircularFalloff(Vector3 worldPos, float radius, float power, int centerX, int centerY, int pixelRadius)
    {
        int minX = Mathf.Max(0, centerX - pixelRadius);
        int maxX = Mathf.Min(maskResolution - 1, centerX + pixelRadius);
        int minY = Mathf.Max(0, centerY - pixelRadius);
        int maxY = Mathf.Min(maskResolution - 1, centerY + pixelRadius);

        bool changed = false;
        float dirtRemovedThisCall = 0f;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = (x - centerX) / (float)maskResolution * worldWidth;
                float dy = (y - centerY) / (float)maskResolution * worldHeight;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                if (distance <= radius)
                {
                    float distance01 = distance / radius;
                    float strength = falloffCurve.Evaluate(1f - distance01);
                    float cleanAmount = Mathf.Clamp01(power * strength);

                    float oldVal = dirtiness[x, y];
                    if (oldVal > 0f && cleanAmount > 0f)
                    {
                        float newVal = Mathf.Max(0f, oldVal - cleanAmount);
                        dirtiness[x, y] = newVal;
                        float removed = oldVal - newVal;
                        dirtRemovedThisCall += removed;
                        totalDirtLeft -= removed;
                        changed = true;
                    }
                }
            }
        }

        if (!changed)
            return;

        UpdateMaskRegion(minX, minY, maxX, maxY);

        float percent = initialTotalDirt > 0f ? (1f - totalDirtLeft / initialTotalDirt) * 100f : 100f;
        if (Mathf.Abs(percent - lastReportedPercent) > 0.1f)
        {
            lastReportedPercent = percent;
            OnCleanProgress?.Invoke(percent);
        }

        OnCleaned?.Invoke(new CleanResultInfo
        {
            dirt = this,
            worldPosition = worldPos,
            radius = radius,
            power = power,
            dirtRemoved = dirtRemovedThisCall,
            totalCleanPercent = percent
        });
    }

    // --- Управление состоянием ---

    public void Clear()
    {
        for (int y = 0; y < maskResolution; y++)
            for (int x = 0; x < maskResolution; x++)
                dirtiness[x, y] = 0f;

        totalDirtLeft = 0f;
        UpdateFullMask();

        float percent = 100f;
        if (Mathf.Abs(percent - lastReportedPercent) > 0.1f)
        {
            lastReportedPercent = percent;
            OnCleanProgress?.Invoke(percent);
        }

        OnCleaned?.Invoke(new CleanResultInfo
        {
            dirt = this,
            worldPosition = transform.position,
            radius = Mathf.Max(worldWidth, worldHeight),
            power = 1f,
            dirtRemoved = initialTotalDirt,
            totalCleanPercent = 100f
        });
    }

    public void Reset()
    {
        for (int y = 0; y < maskResolution; y++)
            for (int x = 0; x < maskResolution; x++)
                dirtiness[x, y] = initialDirtiness[x, y];

        totalDirtLeft = initialTotalDirt;
        UpdateFullMask();

        float percent = 0f;
        if (Mathf.Abs(percent - lastReportedPercent) > 0.1f)
        {
            lastReportedPercent = percent;
            OnCleanProgress?.Invoke(percent);
        }
    }

    // --- Вспомогательные методы ---

    private void UpdateFullMask()
    {
        Color[] pixels = new Color[maskResolution * maskResolution];
        int i = 0;
        for (int y = 0; y < maskResolution; y++)
            for (int x = 0; x < maskResolution; x++)
                pixels[i++] = new Color(dirtiness[x, y], dirtiness[x, y], dirtiness[x, y], 1f);

        maskTex.SetPixels(pixels);
        maskTex.Apply();
    }

    private void UpdateMaskRegion(int minX, int minY, int maxX, int maxY)
    {
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        Color[] pixels = maskTex.GetPixels(minX, minY, width, height);
        int i = 0;
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                pixels[i++] = new Color(dirtiness[x, y], dirtiness[x, y], dirtiness[x, y], 1f);

        maskTex.SetPixels(minX, minY, width, height, pixels);
        maskTex.Apply();
    }
}