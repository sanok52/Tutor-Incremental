using DG.Tweening.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Unity.VisualScripting.Member;
using Random = UnityEngine.Random;

public class ResourceManager
{
    public Dictionary<string, IValueContainer> resources = new Dictionary<string, IValueContainer>();


    public IValueContainer this[string title]
    {
        get => resources[title];
        set => resources[title] = value;
    }

    public void Init(params IValueContainer[] containers)
    {
        foreach (var container in containers)
        {
            var cloneContainer = container.Clone();
            resources.Add(container.Title, cloneContainer);
            Subscrabe(cloneContainer);
            cloneContainer.SetValue(cloneContainer.Value);
        }
    }

    private void Subscrabe(IValueContainer container)
    {
        InterfaceManager.BarMediator.SetMaxForID(container.Title, container.ClampRange.y);
        container.OnChangeValue += (data) => ShowText(data);
    }

    private void ShowText(ContainerChangeData data)
    {
        InterfaceManager.BarMediator.ShowForID(data.Title, data.CurrentValue);

        if (data.Point != null)
            Show3DText(data);

        var bars = InterfaceManager.BarMediator.GetBarsID(data.Title);

        RectTransform canvasRect = InterfaceManager.MainCanvas.GetComponent<RectTransform>();
        Canvas canvas = InterfaceManager.MainCanvas;

        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        foreach (var bar in bars)
        {
            if (bar.TryGetComponent(out RectTransform rectTransform))
            {
                // 1. Берём мировую позицию точки, где хотим текст.
                // Можно pivot, можно центр — возьмём pivot:
                Vector3 worldPos = rectTransform.position;

                // 2. Переводим в экранные координаты
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);

                // 3. Переводим в локальные координаты Canvas (anchoredPosition)
                Vector2 uiPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPos,
                    uiCamera,
                    out uiPos
                );

                var localData = new ContainerChangeData(data)
                {
                    Point = uiPos   // это уже anchoredPosition для mainCanvas
                };

                ShowUIText(localData);
            }
        }
    }
    private void Show3DText(ContainerChangeData data)
    {
        if (data.GetPoint() == null)
            return;

        InterfaceManager.CreateScoreFlyingText((int)data.DeltaNotClamp, data.GetPoint().Value +
        (Random.insideUnitSphere * 0.1f),
        Mathf.Lerp(0.25f, 1.5f, (data.Delta) / 500f));
    }

    private void ShowUIText(ContainerChangeData data)
    {
        Vector2 basePos = data.GetPoint().Value; // anchoredPosition в Canvas

        Vector2 offset = Random.insideUnitCircle * 30f; // разброс в пикселях
        Vector2 finalPos = basePos + offset;

        InterfaceManager.CreateDeltaFlyingText(
            (int)data.DeltaNotClamp,
            data.InterfaceSettings,
            new Vector3(finalPos.x, finalPos.y, 0f),
            null,
            true // isUI
        );
    }

    public void AddResource(string id, int add, Vector3? position, Transform transform)
    {
        resources[id].AddValue(add, position, transform, add > 0 ? ValueChangeType.Add : ValueChangeType.Remove);
    }
}