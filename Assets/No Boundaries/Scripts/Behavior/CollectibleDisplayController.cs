using UnityEngine;
using System.Collections;

public class CollectibleDisplayController : MonoBehaviour {

    public virtual void Show()
    {

    }

    public virtual void Hide()
    {
        GameObject.Destroy(gameObject);
    }
}
