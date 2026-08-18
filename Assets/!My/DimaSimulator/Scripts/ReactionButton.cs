using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReactionButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;          // показывает количество реакций на посте
    [SerializeField] private GameObject cooldownOverlay; // затемнение / иконка замка

    private ReactionType reactionType;
    private Post targetPost;
    private PlayerInGame playerData;

    public void Initialize(ReactionType type, Post post)
    {
        reactionType = type;
        targetPost = post;
        playerData = G.PlayerInGame;

        if (playerData == null)
        {
            Debug.LogError("PlayerGameData не найден!");
            return;
        }

        // Иконка
        var info = GetTypeInfo(type);
        if (info != null && info.icon != null)
            iconImage.sprite = info.icon;

        // Подписка на события игрока (обновление доступности)
        playerData.OnReactionCountChanged += OnPlayerCountChanged;
        playerData.OnReactionCooldownStarted += OnCooldownStarted;
        playerData.OnReactionCooldownEnded += OnCooldownEnded;

        // Начальное обновление
        UpdateUI();

        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (playerData == null || targetPost == null)
            return;

        // Если реакция на кулдауне – не даём ставить
        if (playerData.IsRecharging(reactionType))
        {
            //Debug.Log($"Реакция {reactionType} на кулдауне");
            return;
        }

        // Попытка поставить реакцию (PlayerGameData сам уменьшит заряд и запустит кулдаун)
        playerData.TryPlaceReaction(targetPost, reactionType);

        UpdatePostReactionCount();
        UpdateAvailability(); // обновить доступность кнопки
    }

    // ---- Обновление счётчика реакций на посте ----
    private void UpdatePostReactionCount()
    {
        if (targetPost != null)
        {
            int count = targetPost.GetReactionCount(reactionType);
            countText.text = count > 0 ? count.ToString() : "";
        }
    }

    // ---- Обновление доступности кнопки (зависит от зарядов и кулдауна) ----
    private void UpdateAvailability()
    {
        if (playerData == null) return;

        int charges = playerData.GetCurrentCount(reactionType);
        bool isRecharging = playerData.IsRecharging(reactionType);

        // Кнопка доступна, если есть заряды И не на кулдауне
        bool interactable = charges > 0 && !isRecharging;
        button.interactable = interactable;

        if (cooldownOverlay != null)
            cooldownOverlay.SetActive(isRecharging);
    }

    // ---- Полное обновление UI (счётчик + доступность) ----
    private void UpdateUI()
    {
        UpdatePostReactionCount();
        UpdateAvailability();
    }

    // ---- Обработчики событий от PlayerGameData ----
    private void OnPlayerCountChanged(ReactionType type, int newCount)
    {
        if (type != reactionType) return;
        UpdateAvailability(); // изменилось количество зарядов – обновляем доступность
    }

    private void OnCooldownStarted(ReactionType type)
    {
        if (type != reactionType) return;
        UpdateAvailability();
    }

    private void OnCooldownEnded(ReactionType type)
    {
        if (type != reactionType) return;
        UpdateAvailability();
    }

    private ReactionTypeInfo GetTypeInfo(ReactionType type)
    {
        return G.Data?.allReactionTypes.Find(info => info.type == type);
    }

    private void OnDestroy()
    {
        if (playerData != null)
        {
            playerData.OnReactionCountChanged -= OnPlayerCountChanged;
            playerData.OnReactionCooldownStarted -= OnCooldownStarted;
            playerData.OnReactionCooldownEnded -= OnCooldownEnded;
        }
        button.onClick.RemoveAllListeners();
    }
}