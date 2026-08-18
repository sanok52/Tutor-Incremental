using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System;
using DG.Tweening;

public enum InterfaceComand { None, SwitchPause, OpenPause, ClosePause, PlayGame, Exit, GoToMenu }

public static class InterfaceManager
{
    public static Canvas mainCanvas;
    private static BlackScreen blackScreen;
    public static TextScoreUpLR flyingTextPref;
    public static TextScoreUpLR flyingTextUIPref;
    public static TextSpriteScoreUpLR flyingSpriteTextPref;
    public static TextSpriteScoreUpLR flyingSpriteTextUIPref;

    public static BarUIMediator BarMediator { get; private set; }
    public static Canvas MainCanvas => mainCanvas;

    public static BlackScreen MainBlackScreen => blackScreen;
    public static ColorBetweenScore DefaultColor => new ColorBetweenScore() { GreaterZero = Color.yellow, LassZero = Color.red, Zero = Color.gray };

    private static readonly List<WindowUI> createdWindows = new();

    public static Action<WindowUI> OnClose;
    public static Action<WindowUI> OnOpen;

    public static Action<InterfaceComand> OnClickCommand;

    /*──────────────────────────  INIT  ──────────────────────────*/

    public static void Init(Canvas canvasOverride = null)
    {
        OnOpen = null;
        OnClose = null;
        OnClickCommand = null;
        createdWindows.Clear();
        blackScrees.Clear();

        mainCanvas = canvasOverride ??
                     GameObject.FindGameObjectWithTag("MainCanvas")?.GetComponent<Canvas>();

        if (mainCanvas == null)
        {
            Debug.LogError("InterfaceManager: Canvas not found. Tag a canvas with 'MainCanvas' or pass it explicitly.");
            //return;
        }

        flyingTextPref = Resources.Load<TextScoreUpLR>("InterfacePrefabs/FlyingText");
        flyingTextUIPref = Resources.Load<TextScoreUpLR>("InterfacePrefabs/FlyingTextUI");
        flyingSpriteTextPref = Resources.Load<TextSpriteScoreUpLR>("InterfacePrefabs/FlyingSpriteText");
        flyingSpriteTextUIPref = Resources.Load<TextSpriteScoreUpLR>("InterfacePrefabs/FlyingUISpriteText");

        BarMediator = new BarUIMediator();
        BarMediator.UpdateLinks();

        blackScreen = Resources.Load<BlackScreen>("InterfacePrefabs/BlackScreen");
    }

    /*─────────────────────  WINDOW FACTORY  ─────────────────────*/

    public static T CreateWindow<T>(T windowUIPref) where T : WindowUI
    {
        Debug.Log($"CreateWindow {typeof(T).ToString()}");
        if (mainCanvas == null || windowUIPref == null)
            return null;

        // Если уже открыто окно того же типа – просто активировать
        foreach (var w in createdWindows)
        {
            if (w is T existing)
            {
                existing.Open();          // «поднять» окно
                //blackScreen.Show();
                OnOpen?.Invoke(existing);
                return existing;
            }
        }

        T window = GameObject.Instantiate(windowUIPref, mainCanvas.transform);
        window.Open();
        createdWindows.Add(window);
        //blackScreen.Show();
        OnOpen?.Invoke(window);
        return window;
    }

    public static void CloseWindowWork(WindowUI window)
    {
        /*bool hideBlack = true;
        foreach (var w in createdWindows)
        {
            if (w.IsOpen && w.UseBlackScreen && w != window)
            {
                hideBlack = false;
                break;
            }
        }
        if(hideBlack && blackScreen != null)
            blackScreen.Hide();*/

        OnClose?.Invoke(window);
    }

    /*──────────────────────── SHOW HELPERS ──────────────────────*/

    public static T CreateAndShowWindow<T>() where T : WindowUI
    {
        T pref = Resources.Load<T>($"InterfacePrefabs/{typeof(T)}");

        if (pref == null)
        {
            Debug.LogError($"InterfaceManager: {typeof(T).ToString()} Prefabs not found in Resources/InterfacePrefabs.");
            return null;
        }

        var window = CreateWindow(pref);
        return window;
    }

    /*───────────────────────  HOUSEKEEPING  ─────────────────────*/

    /// <summary>Закрывает и уничтожает все открытые окна.</summary>
    public static void ClearAllWindows()
    {
        foreach (var w in createdWindows.ToArray())
        {
            if (w == null) continue;
            w.Close();
            GameObject.Destroy(w.gameObject);
        }
        createdWindows.Clear();
    }

    /*────────────────────────  BUTTON API  ──────────────────────*/

    public static void ClickButton(InterfaceComand cmd)
    {
        OnClickCommand?.Invoke(cmd);
    }

    /*──────────────────── OPTIONAL: UI CHECK ────────────────────*/

    /// <summary>True, если курсор/тап над UI-элементом.</summary>
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (Application.isMobilePlatform)
            return Input.touchCount > 0 &&
                   EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return EventSystem.current.IsPointerOverGameObject();
    }

    public static void CreateScoreFlyingText(int scoreChange, Vector3 point)
    {
        CreateScoreFlyingText(scoreChange, point, null, false, null);
    }

    public static void CreateScoreFlyingText(int scoreChange, Vector3 point,
        float? sizeKof)
    {
        CreateScoreFlyingText(scoreChange, point, null, false, sizeKof: sizeKof);
    }

    public static void CreateScoreFlyingText(int scoreChange, Vector3 point, Transform parent, bool isUI,
        float? sizeKof = null)
    {
        CreateDeltaFlyingText("", scoreChange, "", point, parent, new ColorBetweenScore(), isUI, sizeKof: sizeKof);
    }

    public static void CreateDeltaFlyingText(int scoreChange, ValueContainerUISettings uISettings, Vector3 point, Transform parent, bool isUI = false)
    {
        CreateDeltaFlyingText(uISettings.Prefix, scoreChange, uISettings.Postfix, point, parent, uISettings.Color, isUI, uISettings.SizeKof > 0 ? uISettings.SizeKof : null);
    }

    public static void CreateDeltaFlyingText(string prefix, int scoreChange, string postfix, Vector3 point, Transform parent, ColorBetweenScore colorBetween, bool isUI = false,
        float? sizeKof = null)
    {
        Color finalColor = colorBetween.GetColor(scoreChange);
        if (finalColor == default)
            finalColor = DefaultColor.GetColor(scoreChange);

        if (string.IsNullOrEmpty(prefix))
            prefix = "";
        if (string.IsNullOrEmpty(postfix))
            postfix = "";

        if (scoreChange > 0)
        {
            prefix += "+";
        }
        else
        if (scoreChange == 0)
        {
            prefix += "+";
        }
        else
        if (scoreChange < 0)
        {
        }

        TextScoreUpLR textScoreUp = CreateFlyingText(prefix + scoreChange.ToString() + postfix, finalColor, point, parent, isUI);
        if (textScoreUp && sizeKof != null)
        {
            textScoreUp.transform.localScale *= (float)sizeKof;
        }
    }

    public static TextScoreUpLR CreateFlyingText(
    string text,
    Color color,
    Vector3 point,
    Transform parent,
    bool isUI = false)
    {
        TextScoreUpLR textScoreUp;

        if (isUI)
        {
            // point — это anchoredPosition в координатах Canvas
            Transform canvas = parent ? parent : mainCanvas.transform;

            textScoreUp = GameObject.Instantiate(flyingTextUIPref, canvas);
            var rt = textScoreUp.GetComponent<RectTransform>();

            rt.anchoredPosition = new Vector2(point.x, point.y);

            textScoreUp.Init(text, color, rt.position);
        }
        else
        {
            // point — мировая позиция
            textScoreUp = GameObject.Instantiate(flyingTextPref, point, Quaternion.identity, parent);
            textScoreUp.Init(text, color, point);
        }

        GameObject.Destroy(textScoreUp.gameObject, textScoreUp.DurationFly);
        return textScoreUp;
    }

    public static void CreateDeltaSpriteFlyingText(Sprite sprite, string prefix, int scoreChange, string postfix, Vector3 point, Transform parent, ColorBetweenScore colorBetween, bool isUI = false,
        float? sizeKof = null)
    {
        Color finalColor = colorBetween.GetColor(scoreChange);
        if (finalColor == default)
            finalColor = DefaultColor.GetColor(scoreChange);

        if (string.IsNullOrEmpty(prefix))
            prefix = "";
        if (string.IsNullOrEmpty(postfix))
            postfix = "";

        if (scoreChange > 0)
        {
            prefix += "+";
        }
        else
        if (scoreChange == 0)
        {
            prefix += "+";
        }
        else
        if (scoreChange < 0)
        {
        }

        TextScoreUpLR textScoreUp = CreateFlyingSpriteText(sprite, prefix + scoreChange.ToString() + postfix, finalColor, point, parent, isUI);
        if (textScoreUp && sizeKof != null)
        {
            textScoreUp.transform.localScale *= (float)sizeKof;
        }
    }

    public static TextSpriteScoreUpLR CreateFlyingSpriteText (
        Sprite sprite, 
        string text,
        Color color,
    Vector3 point,
    Transform parent,
    bool isUI = false)
    {
        TextSpriteScoreUpLR textScoreUp;

        if (isUI)
        {
            textScoreUp = GameObject.Instantiate(flyingSpriteTextUIPref, point, Quaternion.identity, parent);
            Transform canvas = parent ? parent : mainCanvas.transform;

            textScoreUp.transform.parent = canvas;
            var rt = textScoreUp.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(point.x, point.y);
            point = rt.position;
        }
        else
        {
            textScoreUp = GameObject.Instantiate(flyingSpriteTextPref, point, Quaternion.identity, parent);
        }
        textScoreUp.Init(text, color, point);
        textScoreUp.SetSprite(sprite);

        GameObject.Destroy(textScoreUp.gameObject, textScoreUp.DurationFly);
        return textScoreUp;
    }

    public static void ShowBlackScreen(RectTransform rectTrWindow)
    {
        if (MainBlackScreen == null)
            return;

        BlackScreen blackScreen = UnityEngine.Object.Instantiate(MainBlackScreen, rectTrWindow.parent);
        blackScreen.transform.SetSiblingIndex(rectTrWindow.GetSiblingIndex());
        blackScreen.Show();

        blackScrees.Add(rectTrWindow, blackScreen);
    }

    public static void HideBlackScreen(RectTransform rectTrWindow)
    {
        if (blackScrees.TryGetValue(rectTrWindow, out BlackScreen blackScreen))
        {
            blackScreen.Hide(true);
            blackScrees.Remove(rectTrWindow);
        }
    }

    private static Dictionary<RectTransform, BlackScreen> blackScrees = new Dictionary<RectTransform, BlackScreen>();

    /// <summary>
    /// Возвращает локальную позицию (anchoredPosition) RectTransform относительно его родителя.
    /// </summary>
    public static Vector2 GetUIPosition(RectTransform target)
    {
        return target.anchoredPosition;
    }

    /// <summary>
    /// Возвращает позицию (anchoredPosition), которую нужно задать объекту positor,
    /// чтобы он оказался в той же мировой позиции, что и target.
    /// Учитывает разные родительские контейнеры.
    /// </summary>
    public static Vector2 GetUIPosition(RectTransform target, RectTransform positor)
    {
        // Если родители совпадают – просто копируем позицию
        if (target.parent == positor.parent)
            return target.anchoredPosition;

        // Получаем мировую позицию target
        Vector3 worldPos = target.position;

        // Находим Canvas, в котором находится positor
        Canvas canvas = positor.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("positor не находится внутри Canvas!");
            return Vector2.zero;
        }

        Camera cam = canvas.worldCamera;
        // Для режима ScreenSpaceOverlay камера не используется
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            cam = null;

        // Преобразуем мировую позицию в экранные координаты
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

        // Преобразуем экранные координаты в локальные координаты родителя positor
        RectTransform parentRect = positor.parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogError("Родитель positor не является RectTransform!");
            return Vector2.zero;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPoint,
            cam,
            out Vector2 localPos
        );

        return localPos;
    }

    /// <summary>
    /// Возвращает локальную позицию (anchoredPosition) для объекта targetRect,
    /// чтобы он оказался в точке экрана screenPoint (например, позиция мыши).
    /// </summary>
    public static Vector2 GetUIPositionFromScreenPoint(Vector2 screenPoint, RectTransform targetRect)
    {
        Canvas canvas = targetRect.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("targetRect не находится внутри Canvas!");
            return Vector2.zero;
        }

        Camera cam = canvas.worldCamera;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            cam = null;

        RectTransform parentRect = targetRect.parent as RectTransform;
        if (parentRect == null)
        {
            Debug.LogError("Родитель targetRect не является RectTransform!");
            return Vector2.zero;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPoint,
            cam,
            out Vector2 localPos
        );

        return localPos;
    }

    /// <summary>
    /// Упрощённая версия для ScreenSpaceOverlay (без камеры).
    /// Возвращает экранные координаты как они есть (их можно использовать как anchoredPosition в Overlay).
    /// </summary>
    public static Vector2 GetUIPositionFromScreenPoint(Vector2 screenPoint)
    {
        return screenPoint;
    }
}

[Serializable]
public struct ColorBetweenScore
{

    public Color LassZero;
    public Color Zero;
    public Color GreaterZero;

    public Color GetColor(int score)
    {
        if (score < 0)
            return LassZero;
        else if (score == 0)
            return Zero;
        else
            return GreaterZero;
    }
}