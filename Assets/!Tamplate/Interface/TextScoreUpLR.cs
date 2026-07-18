using UnityEngine;
using TMPro;
using DG.Tweening;
public class TextScoreUpLR : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _moveDuration = 1f;
    [SerializeField] private float speedOffset = 2f;
    [SerializeField] private float _moveDistance = 1f;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private float _delayBeforeFade = 0.5f;
    [SerializeField] private Ease _moveEase = Ease.OutQuad;

    [Header("References")]
    [SerializeField] private bool isUI = false;
    [SerializeField] private TMP_Text _textMesh;

    [Space]
    [SerializeField] private bool initToStart = false;

    private Vector3 _initialPosition;
    private Color _initialColor;
    private Vector3 center;

    public float DurationFly => _moveDuration + _fadeDuration + 0.1f;

    private void Awake()
    {
        // Сохраняем начальные значения
        _initialPosition = transform.position;
        _initialColor = _textMesh.color;

        if (initToStart)
            Init(_textMesh.text, _textMesh.color, transform.position);
    }

    public void Init(string text, Color color, Vector3 center)
    {
        // Устанавливаем текст и цвет
        _textMesh.text = text;
        _textMesh.color = color;
        this.center = center;

        // Сбрасываем позицию и прозрачность
        _initialPosition = transform.position;
        _textMesh.color = new Color(color.r, color.g, color.b, 1f);

        // Запускаем анимацию
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        // Последовательность анимаций
        Sequence sequence = DOTween.Sequence();

        // Движение вверх
        Vector3 moveOffset = Vector3.right * Mathf.Clamp((center - transform.position).x, -1f, 1f);
        if (isUI) {
            TryGetComponent(out RectTransform rectTransform);
            sequence.Append(rectTransform.DOAnchorPos(rectTransform.anchoredPosition + (Vector2.up * _moveDistance) /*+ (moveOffset * speedOffset)*/,
                _moveDuration)
                .SetEase(_moveEase));
        }
        else
            sequence.Append(transform.DOMove(transform.position + (Vector3.up * _moveDistance) /*+ (moveOffset * speedOffset)*/,
                _moveDuration)
                .SetEase(_moveEase));

        // Задержка перед исчезновением
        sequence.AppendInterval(_delayBeforeFade);

        // Исчезновение
        sequence.Append(_textMesh.DOFade(0f, _fadeDuration));

        // Уничтожение объекта после завершения
        sequence.OnComplete(() => Destroy(gameObject));
    }

    // Метод для сброса состояния (может быть полезен при использовании пула объектов)
    public void ResetState()
    {
        transform.position = _initialPosition;
        _textMesh.color = _initialColor;
    }
}