using System;
using System.Linq;

[Serializable]
public struct InfoElement
{
    public string[] all;

    /// <summary>ѕровер€ет, содержит ли этот элемент все строки из другого InfoElement.</summary>
    public bool Contains(InfoElement other)
    {
        if (other.all == null || other.all.Length == 0) return true; // пустой набор считаетс€ содержащимс€
        if (all == null || all.Length == 0) return false;
        foreach (string tag in other.all)
        {
            if (!Contains(tag)) return false;
        }
        return true;
    }

    public bool Contains(string[] tags)
    {
        if (all.All(x => tags.Contains(x)))
            return true;
        return false;
    }

    private bool Contains(string tag)
    {
        if (all == null) return false;
        for (int i = 0; i < all.Length; i++)
        {
            if (string.Equals(all[i], tag, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}
