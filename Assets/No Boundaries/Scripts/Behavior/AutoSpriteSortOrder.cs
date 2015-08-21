using UnityEngine;
using System.Collections;

public class AutoSpriteSortOrder : MonoBehaviour
{
    private new Renderer renderer;
    public Renderer Renderer
    {
        get
        {
            if (renderer == null) renderer = GetComponent<Renderer>();
            return renderer;
        }

    }

    public int OrderInGroup;

    public void SetOrder(int offset)
    {
        if (Renderer == null) Debug.LogErrorFormat("No Renderer attached to {0}", name);
        Renderer.sortingOrder = offset + OrderInGroup;
    }
}
