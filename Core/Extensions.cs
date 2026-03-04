using UnityEngine;

namespace APreds.Core;

public static class Extensions
{
    public static void Obliterate(this Component comp)
    {
        if (comp != null) UnityEngine.Object.Destroy(comp);
    }
}