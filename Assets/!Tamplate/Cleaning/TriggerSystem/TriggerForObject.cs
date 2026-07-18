using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerForObject : MonoBehaviour
{
    public List<string> tags = new List<string>(); // теги самой зоны

    public event Action<TriggerObjectMessageReciver> OnItemEnter;
    public event Action<TriggerObjectMessageReciver> OnItemExit;
    public event Action<TriggerObjectMessageReciver> OnItemStay;

    private List<TriggerObjectMessageReciver> triggerObjects = new List<TriggerObjectMessageReciver>();
    private List<TriggerObjectMessageReciver> toRemove = new List<TriggerObjectMessageReciver>(); // дл€ безопасного удалени€

    private void Update()
    {
        // ѕеребираем копию, чтобы можно было безопасно мен€ть список внутри колбэков
        foreach (var item in triggerObjects)
        {
            if (item == null)
            {
                // ќбъект мог быть уничтожен Ц запланируем удаление
                toRemove.Add(item);
                continue;
            }
            StayItem(item);
        }

        // „истим список от null-ссылок (на случай уничтожени€ объекта во врем€ нахождени€ в зоне)
        if (toRemove.Count > 0)
        {
            foreach (var rem in toRemove)
                triggerObjects.Remove(rem);
            toRemove.Clear();
        }
    }

    private void StayItem(TriggerObjectMessageReciver item)
    {
        OnItemStay?.Invoke(item);
        item.InvokeStay(tags.ToArray(), false); // оповещаем сам объект, что он всЄ ещЄ внутри
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out TriggerObjectMessageReciver triggerObject))
        {
            if (triggerObjects.Contains(triggerObject))
                return;

            if (!triggerObject.TestAnyTags(tags.ToArray()))
                return;

            triggerObjects.Add(triggerObject);

            OnItemEnter?.Invoke(triggerObject);
            triggerObject.InvokeEnter(this, tags.ToArray(), false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out TriggerObjectMessageReciver itemObject))
        {
            ExitItem(itemObject);
        }
    }

    public void ExitItem(TriggerObjectMessageReciver itemObject)
    {
        if (!triggerObjects.Contains(itemObject))
            return;

        triggerObjects.Remove(itemObject);

        OnItemExit?.Invoke(itemObject);
        itemObject.InvokeExit(this, tags.ToArray(), false); // оповещаем объект о выходе
    }
}
