using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Автоматически корректирует позицию скролла при изменении размера Content.
/// Если пользователь у края (нижнего или верхнего), скролл остаётся у этого края.
/// Если пользователь в середине, видимая область фиксируется (не сбивает с чтения).
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ScrollRectTracker : MonoBehaviour
{
    [Tooltip("Следить за нижним краем (true — для чатов, false — для верхнего края).")]
    [SerializeField] private bool followBottom = true;

    [Tooltip("Порог (0..1) для определения нахождения у края.")]
    [SerializeField] private float edgeThreshold = 0.01f;

    private ScrollRect _scrollRect;
    private RectTransform _viewport;
    private RectTransform _content;

    private bool _isAtEdge;          // находится ли пользователь у края
    private float _offsetFromTop;    // абсолютное смещение от верха Content (в пикселях)
    private float _lastContentHeight;
    private bool _initialized;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _viewport = _scrollRect.viewport;
        _content = _scrollRect.content;
        if (_viewport == null || _content == null)
        {
            Debug.LogError("Viewport или Content не назначены!", this);
            enabled = false;
        }
    }

    private void Start()
    {
        _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        _initialized = true;
        UpdateState();
        _lastContentHeight = _content.rect.height;
    }

    private void OnDestroy()
    {
        _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
    }

    // Обновление состояния при скролле
    private void OnScrollValueChanged(Vector2 pos)
    {
        if (!_initialized) return;
        UpdateState();
    }

    // Определение текущего режима и сохранение параметров
    private void UpdateState()
    {
        float y = _scrollRect.normalizedPosition.y;
        float contentHeight = _content.rect.height;
        float viewportHeight = _viewport.rect.height;

        if (contentHeight <= viewportHeight || Mathf.Approximately(contentHeight, 0f))
        {
            _isAtEdge = true;
            _offsetFromTop = 0;
            return;
        }

        // Проверка нахождения у края (с учётом followBottom)
        bool nearEdge;
        if (followBottom)
            nearEdge = y <= edgeThreshold;          // нижний край = y = 0
        else
            nearEdge = y >= 1f - edgeThreshold;     // верхний край = y = 1

        if (nearEdge)
        {
            _isAtEdge = true;
            _offsetFromTop = 0; // не используется
        }
        else
        {
            _isAtEdge = false;
            // Сохраняем абсолютное смещение от верха Content
            _offsetFromTop = y * (contentHeight - viewportHeight);
        }
    }

    // Отслеживание изменения размера Content
    private void LateUpdate()
    {
        if (!_initialized) return;

        float currentHeight = _content.rect.height;
        if (Mathf.Approximately(currentHeight, _lastContentHeight))
            return;

        _lastContentHeight = currentHeight;
        ApplyCorrection();
    }

    // Применение коррекции позиции
    private void ApplyCorrection()
    {
        // Принудительно обновляем Canvas, чтобы все Layout пересчитались
        Canvas.ForceUpdateCanvases();

        float contentHeight = _content.rect.height;
        float viewportHeight = _viewport.rect.height;

        if (contentHeight <= viewportHeight || Mathf.Approximately(contentHeight, 0f))
        {
            // Если контент меньше вьюпорта — прижимаем к выбранному краю
            _scrollRect.normalizedPosition = new Vector2(_scrollRect.normalizedPosition.x, followBottom ? 0f : 1f);
            return;
        }

        if (_isAtEdge)
        {
            // Режим слежения за краем
            float targetY = followBottom ? 0f : 1f;
            _scrollRect.normalizedPosition = new Vector2(_scrollRect.normalizedPosition.x, targetY);
        }
        else
        {
            // Режим фиксации позиции — восстанавливаем сохранённое смещение
            float newY = _offsetFromTop / (contentHeight - viewportHeight);
            newY = Mathf.Clamp01(newY);
            _scrollRect.normalizedPosition = new Vector2(_scrollRect.normalizedPosition.x, newY);
        }
    }
}