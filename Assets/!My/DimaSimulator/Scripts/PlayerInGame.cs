using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInGame : MonoBehaviour
{
    [Header("Настройки реакций")]
    public List<ReactionSetting> reactionSettings = new List<ReactionSetting>();

    [Header("Подписки на авторов")]
    public List<Author> subscribedAuthors = new List<Author>();

    private List<Post> repostingPosts = new List<Post>();

    // Внутренние данные
    private Dictionary<ReactionType, int> currentCounts = new Dictionary<ReactionType, int>();
    private Dictionary<ReactionType, int> maxCounts = new Dictionary<ReactionType, int>();
    private Dictionary<ReactionType, float> cooldowns = new Dictionary<ReactionType, float>();
    private Dictionary<ReactionType, bool> isRecharging = new Dictionary<ReactionType, bool>();
    private bool isRechargingRepost = false;

    // События для UI (оповещают об изменении состояния)
    public Action<ReactionType, int> OnReactionCountChanged;     // (тип, новый заряд)
    public Action<ReactionType> OnReactionCooldownStarted;      // кулдаун начался
    public Action<ReactionType> OnReactionCooldownEnded;        // кулдаун закончился
    public Action OnReactionRepostCooldownStarted;      // кулдаун начался
    public Action OnReactionRepostCooldownEnded;        // кулдаун закончился
    public Action<Post, ReactionType, int> OnPlayerPostReaction; //реакция игрока на пост

    private int starsUp = 2;
    public bool IsRechargingRepost => isRechargingRepost;

    private void Awake()
    {
        foreach (var setting in reactionSettings)
        {
            currentCounts[setting.type] = setting.maxCount;
            maxCounts[setting.type] = setting.maxCount;
            cooldowns[setting.type] = setting.cooldown;
            isRecharging[setting.type] = false;
        }
    }

    private void Start()
    {
        G.ResourceManager["Likes"].OnOverfullValue += LikesOverfull;

        TestButtons.SubOnClick("CompliteMisson", (info) =>
        {
            Debug.Log("ClickComplite");
            G.MissionManager.Missions[0].Complete();
        });
    }

    private void LikesOverfull(int over)
    {
        G.ResourceManager.AddResource("Stars", starsUp, null, null);
        starsUp *= 2;
        G.ResourceManager["Likes"].Init(G.ResourceManager["Likes"].Value + over, new Vector2Int(0, G.ResourceManager["Likes"].ClampRange.y * 2));
        InterfaceManager.BarMediator.SetMaxForID("Likes", G.ResourceManager["Likes"].ClampRange.y);
    }

    // ---- Добавление/обновление реакции через ReactionSetting ----
    public void AddReaction(ReactionSetting setting)
    {
        var existing = reactionSettings.Find(s => s.type == setting.type);
        if (existing != null)
        {
            existing.maxCount = setting.maxCount;
            existing.cooldown = setting.cooldown;
            maxCounts[setting.type] = setting.maxCount;
            if (currentCounts[setting.type] > setting.maxCount)
                currentCounts[setting.type] = setting.maxCount;
            cooldowns[setting.type] = setting.cooldown;
            isRecharging[setting.type] = false;
            OnReactionCountChanged?.Invoke(setting.type, currentCounts[setting.type]);
            Debug.Log($"Реакция {setting.type} обновлена");
        }
        else
        {
            reactionSettings.Add(setting);
            currentCounts[setting.type] = setting.maxCount;
            maxCounts[setting.type] = setting.maxCount;
            cooldowns[setting.type] = setting.cooldown;
            isRecharging[setting.type] = false;
            OnReactionCountChanged?.Invoke(setting.type, currentCounts[setting.type]);
            Debug.Log($"Реакция {setting.type} добавлена");
        }
    }

    // ---- Попытка поставить реакцию ----
    public bool TryPlaceReaction(Post post, ReactionType type)
    {
        if (!cooldowns.ContainsKey(type))
        {
            //Debug.LogWarning($"Реакция {type} не доступна");
            return false;
        }

        if (isRecharging[type])
        {
            //Debug.Log($"Реакция {type} на кулдауне");
            return false;
        }

        if (currentCounts[type] <= 0)
        {
            if (!isRecharging[type])
            {
                //Debug.Log($"Реакция {type} – зарядов нет, начинаем кулдаун");
                StartCoroutine(RestoreRecharge(type, cooldowns[type]));
            }
            return false;
        }

        currentCounts[type]--;
        OnReactionCountChanged?.Invoke(type, currentCounts[type]);

        G.InternetBank.PlaceReaction(post, type);

        var info = GetReactionTypeInfo(type);        
        if (info != null)
        {
            float coef = 1f + (post.Rang * 1f);
            if (type == ReactionType.Like)
                coef = 1f;
            info.likesValue = (int)(info.likesValue * coef);

            string pstfix = coef == 1f ? "" : $"<color=orange> <size={Mathf.Clamp(10*coef, 15, 75)}>(x{System.Math.Round(coef, 1)})</size></color>";
            G.ResourceManager.AddResource("Likes", info.likesValue, null, null, postfix: pstfix);

            InterfaceManager.CreateDeltaFlyingText("", info.likesValue, pstfix, 
                InterfaceManager.GetUIPositionFromScreenPoint(Input.mousePosition) + (UnityEngine.Random.insideUnitCircle * 20),           
                null, G.ResourceManager["Likes"].InterfaceData.Color, true, Mathf.Clamp((float)info.likesValue / 10, 0.5f, 50f));

            OnPlayerPostReaction?.Invoke(post, type, info.likesValue);
        }

        if (currentCounts[type] == 0)
        {
            //Debug.Log($"Реакция {type} – заряд закончился, кулдаун {cooldowns[type]}с");
            StartCoroutine(RestoreRecharge(type, cooldowns[type]));
        }

        return true;
    }

    private IEnumerator RestoreRecharge(ReactionType type, float cooldown)
    {
        if (isRecharging[type])
            yield break;

        isRecharging[type] = true;
        OnReactionCooldownStarted?.Invoke(type);

        yield return new WaitForSeconds(cooldown);

        currentCounts[type] = maxCounts[type];
        isRecharging[type] = false;
        OnReactionCountChanged?.Invoke(type, currentCounts[type]);
        OnReactionCooldownEnded?.Invoke(type);

        Debug.Log($"Реакция {type} восстановлена до {maxCounts[type]}");
    }

    private IEnumerator RestoreRechargeRepost(float cooldown)
    {
        if (isRechargingRepost)
            yield break;

        isRechargingRepost = true;
        OnReactionRepostCooldownStarted?.Invoke();

        yield return new WaitForSeconds(cooldown);

        isRechargingRepost = false;
        OnReactionRepostCooldownEnded?.Invoke();
    }

    private ReactionTypeInfo GetReactionTypeInfo(ReactionType type)
    {
        return G.Data?.allReactionTypes.Find(info => info.type == type).Clone();
    }

    // ---- Подписка на авторов ----
    public void SubscribeToAuthor(Author author, Lenta lenta)
    {
        if (!subscribedAuthors.Contains(author))
        {
            author = author.Clone();

            lenta.AddAuthor(author);
            subscribedAuthors.Add(author);
            G.InternetBank.AddAuthor(author);
            ArtistFlow.Instance?.StartArtist(author);
        }
    }

    public void UnsubscribeFromAuthor(Author author)
    {
        if (subscribedAuthors.Remove(author))
        {
            G.InternetBank.RemoveAuthor(author);
            ArtistFlow.Instance?.UnSubAuthor(author);
        }
    }

    // ---- Методы для UI ----
    public int GetCurrentCount(ReactionType type)
    {
        return currentCounts.TryGetValue(type, out int val) ? val : 0;
    }

    public bool IsRecharging(ReactionType type)
    {
        return isRecharging.TryGetValue(type, out bool val) && val;
    }

    public void TryRepost(Post post)
    {

        if (repostingPosts.Any(x => post.image.name == x.image.name))
        {
            InterfaceManager.CreateFlyingText("Повтор!", Color.red, Input.mousePosition, null, true);
            GlobalCooldown(7);
            return;
        }

        SuccessRepost(post);
        GlobalCooldown(3);
    }

    private void SuccessRepost(Post post)
    {
        repostingPosts.Add(post);

        G.ResourceManager.AddResource("Stars", 1, null, null);
        var info = G.ResourceManager["Stars"];
        InterfaceManager.CreateDeltaSpriteFlyingText(G.ResourceManager["Stars"].InterfaceData.Icon, "", 1, "", Input.mousePosition, null, G.ResourceManager["Stars"].InterfaceData.Color,
            true);
    }

    private void GlobalCooldown(float cooldown)
    {
        foreach (var item in reactionSettings)
        {
            StartCoroutine(RestoreRecharge(item.type, cooldown));
            StartCoroutine(RestoreRechargeRepost(cooldown));
        }
    }

    public void AddLikesForMission(Mission mission)
    {
        int count = 0;
        foreach (var target in mission.Targets)
        {
            count += (int)(target.Count * 1.5f);
        }

        G.ResourceManager.AddResource("Likes", count, null, null);

        RectTransform missionWindowRect = G.MissionWindiowUI.GetComponent<RectTransform>();

        InterfaceManager.CreateDeltaSpriteFlyingText(
            G.ResourceManager["Likes"].InterfaceData.Icon,
            "",
            count,
            "",
            missionWindowRect.sizeDelta / 2f,              // теперь это координаты относительно canvasRect
            missionWindowRect,            // явно передаём родителя
            G.ResourceManager["Likes"].InterfaceData.Color,
            true
        );
    }

}