using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// Визуальный объект одного слота. Поддерживает различение левой и правой кнопок мыши.
/// </summary>
public class InventoryObjectViewInvSys : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _spriteIcon;
    [SerializeField] private TMP_Text _ascii;
    [SerializeField] private TMP_Text _textCount;

    private int _slotIndex;
    // Коллбэк теперь принимает индекс и кнопку мыши
    private System.Action<int, PointerEventData.InputButton> _onClickCallback;

    private bool isBlock;
    private ItemDataInvSys itemData;

    public Image SpriteIcon => _spriteIcon;
    public TMP_Text TextCount => _textCount;

    /// <summary>
    /// Инициализация: отображает данные предмета и сохраняет индекс.
    /// </summary>
    public void Init(ItemDataInvSys itemData, int index)
    {
        this.itemData = itemData;

        if (itemData != null)
        {
            _spriteIcon.sprite = itemData.Icon;
            _ascii.text = itemData.ASCII;
            _spriteIcon.color = itemData.Color;
            _ascii.color = itemData.Color;

            _spriteIcon.enabled = true;
        }
        else
        {
            _spriteIcon.sprite = null;
            _ascii.text = "";

            _spriteIcon.enabled = false;
        }
        _slotIndex = index;
        UpdateCount(1);
    }

    public void UpdateCount(int count)
    {
        if (count > 1)
        {
            _textCount.gameObject.SetActive(true);
            _textCount.text = count.ToString();
        }
        else
        {
            _textCount.gameObject.SetActive(false);
        }
    }

    public void SetIndex(int index)
    {
        _slotIndex = index;
    }

    public void SetEmpty()
    {
        _spriteIcon.sprite = null;
        _spriteIcon.enabled = false;
        _textCount.gameObject.SetActive(false);
        _slotIndex = -1;
        _ascii.text = "";
    }

    /// <summary>
    /// Обработчик клика через EventSystem. Вызывает зарегистрированный коллбэк с индексом и кнопкой.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        _onClickCallback?.Invoke(_slotIndex, eventData.button);
    }

    /// <summary>
    /// Подписка на клик слота (используется InventoryViewerInvSys).
    /// </summary>
    public void RegisterClickCallback(System.Action<int, PointerEventData.InputButton> callback)
    {
        _onClickCallback = callback;
    }

    public void UpdateBlockUse (bool blockUse)
    {
        isBlockUse = blockUse;
        UpdateBlockVisual();
    }

    public void SetBlock(bool isBlock)
    {
        this.isBlock = isBlock;
        UpdateBlockVisual();
    }

    private void UpdateBlockVisual()
    {
        if (itemData != null)
        {
            _spriteIcon.color = IsBlock ? itemData.ColorNotUse : itemData.Color;
            _ascii.color = IsBlock ? itemData.ColorNotUse : itemData.Color;
        }
        if (gameObject.activeInHierarchy && TryGetComponent(out Button button))
            button.interactable = !IsBlock;
    }

    private bool isBlockUse;
    public bool IsBlock => isBlock || isBlockUse;
}