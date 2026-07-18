
using System;
using System.Diagnostics;

/// <summary>
/// Представляет конкретный экземпляр (или стек) предмета в слоте инвентаря.
/// </summary>
public class ItemInstanceInvSys
{
    public ItemDataInvSys Data { get; private set; }
    public int Count { get; private set; }          // количество в слоте (если стакается)
    public int RemainingUses { get; private set; }  // оставшиеся использования (если есть лимит)
    public int RemainingUsesMax { get; private set; }  // оставшиеся использования (если есть лимит)

    /// <summary>
    /// Создаёт экземпляр без стека (Count = 1).
    /// </summary>
    public ItemInstanceInvSys(ItemDataInvSys data)
    {
        Data = data;
        Count = 1;
        // если у предмета есть лимит использований и он не удаляется после одного использования,
        // запоминаем начальное количество
        if (!data.DeleteAfterUse && data.UseCount > 0)
        {
            RemainingUses = data.UseCount;
            RemainingUsesMax = data.UseCount;
        }
        else
            RemainingUses = -1; // бесконечно или неважно
    }

    /// <summary>
    /// Увеличивает количество в стеке.
    /// </summary>
    public void AddToStack(int amount)
    {
        Count += amount;
    }

    /// <summary>
    /// Уменьшает стек на указанное количество. Возвращает true, если предмет ещё остался.
    /// </summary>
    public bool RemoveFromStack(int amount)
    {
        Count -= amount;
        return Count > 0;
    }

    /// <summary>
    /// Использует одно использование (если есть лимит). Возвращает true, если предмет исчерпан и должен быть удалён.
    /// </summary>
    public bool UseOne()
    {
        if (RemainingUses > 0)
        {
            RemainingUses--;
            return RemainingUses <= 0;
        }
        return false; // бесконечное использование или не требуется отслеживание
    }

    public void AddUses (int uses)
    {
        RemainingUses += uses;
    }

    public void ReloadUses()
    {
        if (RemainingUses == RemainingUsesMax)
            return;

        RemainingUses = RemainingUsesMax;
    }

    public string CommandAll(string cmd)
    {
        if (Data == null || Data.Behaviour == null)
            return "";

       return Data.Behaviour.ReloadCommandAll(cmd, this);
    }
}