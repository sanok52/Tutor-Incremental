using System;
using System.Linq;

[Serializable]
public struct InfoElementContainer
{
    public InfoElement[] elementsIncluded;
    public InfoElement[] elementsExcluded;

    public bool Contains(params string[] tags)
    {
        // Проверка включений: должен быть хотя бы один подходящий
        bool hasIncluded = false;
        if (elementsIncluded != null && elementsIncluded.Length > 0)
        {
            foreach (var inc in elementsIncluded)
            {
                if (inc.Contains(tags))
                {
                    hasIncluded = true;
                    break;
                }
            }
        }
        else
        {
            // Если нет включений, считаем, что элемент не проходит (нет условий для включения)
            return false;
        }

        if (!hasIncluded) return false;

        // Проверка исключений: ни один excluded не должен полностью содержаться в элементе
        if (elementsExcluded != null)
        {
            foreach (var exc in elementsExcluded)
            {
                if (exc.Contains(tags))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Проверяет, подходит ли элемент под правила контейнера.
    /// Возвращает true, если:
    /// - есть хотя бы один included, который полностью содержится в проверяемом элементе,
    /// - и ни один excluded не содержится полностью в проверяемом элементе.
    /// Если массив included пуст, возвращается false (нет совпадений).
    /// Если массив excluded пуст, он не препятствует.
    /// </summary>
    public bool Contains(InfoElement element)
    {
        // Проверка включений: должен быть хотя бы один подходящий
        bool hasIncluded = false;
        if (elementsIncluded != null && elementsIncluded.Length > 0)
        {
            foreach (var inc in elementsIncluded)
            {
                if (element.Contains(inc))
                {
                    hasIncluded = true;
                    break;
                }
            }
        }
        else
        {
            // Если нет включений, считаем, что элемент не проходит (нет условий для включения)
            return false;
        }

        if (!hasIncluded) return false;

        // Проверка исключений: ни один excluded не должен полностью содержаться в элементе
        if (elementsExcluded != null)
        {
            foreach (var exc in elementsExcluded)
            {
                if (element.Contains(exc))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Проверяет, удовлетворяют ли все элементы из другого контейнера правилам этого контейнера.
    /// То есть для каждого included из other должно выполняться this.Contains(included),
    /// а для каждого excluded из other this.Contains(excluded) должно возвращать false
    /// (т.е. excluded другого контейнера не должны соответствовать правилам этого).
    /// </summary>
    public bool Contains(InfoElementContainer other)
    {
        // Проверяем каждое включение из other
        if (other.elementsIncluded != null)
        {
            foreach (var inc in other.elementsIncluded)
            {
                if (!Contains(inc)) return false; // если хоть одно не подходит, общий результат false
            }
        }

        // Проверяем каждое исключение из other: они не должны подходить под наши правила
        if (other.elementsExcluded != null)
        {
            foreach (var exc in other.elementsExcluded)
            {
                if (Contains(exc)) return false; // если исключение other подходит под наши правила, то отвергаем
            }
        }

        return true;
    }
}