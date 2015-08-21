using UnityEngine;
using System.Collections.Generic;

public class Hud : MonoBehaviour {

    public static Hud FindHud()
    {
        return FindObjectOfType<Hud>();
    }

    public List<Display> Displays;

    public IEnumerable<T> RequestDisplay<T>(string id) where T : Display
    {
        foreach (Display d in Displays)
        {
            if (d.Id == id && d is T) yield return (T)d;
        }
        yield break;
    }
}
