using System;
using UnityEngine;

/// <summary>
///  онфиг предмета (ScriptableObject или простой сериализуемый класс).
/// ’ранит данные, которые одинаковы дл€ всех экземпл€ров этого типа.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/ItemDataInvSys")]
public class ItemDataInvSys : ScriptableObject
{
    [SerializeField] private string _itemId;
    [SerializeField] private string _itemName;
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _itemASCII = "?";
    [SerializeField] private Color _colorIcon = Color.white;
    [SerializeField] private Color _colorIconNotUse = Color.gray;

    [Header("ѕоведение")]
    [SerializeField] private ItemBehContrainerInvSys _behaviour; // поведение предмета
    [SerializeField] private bool _isFinal = false;
    [SerializeField] private bool _isWeapon = false;

    [Header("»спользование")]
    [SerializeField] private bool _deleteAfterUse = true;   // удал€ть ли после одного использовани€
    [SerializeField] private int _useCount = 0;             // 0 Ц бесконечно, >0 Ц количество использований до удалени€ (если deleteAfterUse = false)
    [SerializeField] private bool _canStack = true;         // можно ли стакать в слоте (дл€ режима стакани€ инвентар€)
    [SerializeField] private bool _canUse = true;
    [SerializeField] private bool _canDrop = true;

    [Space]
    public AudioDataPlay dataPlay;

    public string ItemId => _itemId;
    public string ItemName => _itemName;
    public Sprite Icon => _icon;
    public string ASCII => _itemASCII;
    public Color Color => _colorIcon;
    public Color ColorNotUse => _colorIconNotUse;
    public ItemBehContrainerInvSys Behaviour => _behaviour;
    public bool DeleteAfterUse => _deleteAfterUse;
    public int UseCount => _useCount;
    public bool CanStack => _canStack;

    public bool CanDrop => _canDrop;
    public bool CanUse => _canUse;
    public bool IsFinal => _isFinal;
    public bool IsWeapon => _isWeapon;
}