using UnityEngine;

public class RadialProgress3D : MonoBehaviour
{
    [Header("Progress Settings")]
    [SerializeField] private float progress = 0f;  // Текущий прогресс (0 - 1)
    [SerializeField] private float radius = 1f;     // Радиус прогресс-бара
    [SerializeField] private int segments = 100;    // Количество сегментов на прогресс-баре

    [Header("Line Renderer Settings")]
    [SerializeField] private LineRenderer lineRenderer;  // Поле для LineRenderer из инспектора
    [SerializeField] private bool isOveradeColor;
    [SerializeField] private Gradient colorAtProgress;

    public float Progress => progress;

    void Start()
    {
        // Проверяем, есть ли LineRenderer, если нет, добавляем
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Настройка LineRenderer
        lineRenderer.positionCount = segments + 1;  // Количество точек для рисования
        //lineRenderer.widthMultiplier = 0.1f;        // Толщина линии
        lineRenderer.useWorldSpace = false;         // Работать в локальном пространстве

        // Настройка материала и цвета
        //lineRenderer.materialGlass = new Material(Shader.Find("Sprites/Default"));
        //lineRenderer.startColor = Color.green;
        //lineRenderer.endColor = Color.green;

        // Рисуем прогресс-бар
        DrawProgressBar();
    }

    void Update()
    {
        // Обновляем прогресс-бар каждый кадр
        DrawProgressBar();
    }

    void DrawProgressBar()
    {
        // Определяем количество точек, которые будут видны в зависимости от прогресса
        int visibleSegments = Mathf.FloorToInt(progress * segments);

        // Устанавливаем количество точек для LineRenderer
        lineRenderer.positionCount = visibleSegments + 1;

        // Рисуем линии по кругу
        for (int i = 0; i <= visibleSegments; i++)
        {
            // Угловое смещение для каждой точки
            float angle = Mathf.Lerp(0, 2 * Mathf.PI, (float)i / segments);
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            // Показываем точку
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }

        if (isOveradeColor)
        {
            Color c = colorAtProgress.Evaluate(progress);
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;
        }
    }

    // Метод для обновления прогресса (можно вызывать извне)
    public void SetProgress(float newProgress)
    {
        progress = Mathf.Clamp01(newProgress); // Ограничиваем прогресс от 0 до 1
    }

    // Используем OnValidate для обновления в редакторе
    private void OnValidate()
    {
        DrawProgressBar(); // Обновляем прогресс бар в редакторе
    }
}