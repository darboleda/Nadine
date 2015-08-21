using UnityEngine;
using System.Collections;

public class CollectionDisplayer : MonoBehaviour {

    public Transform DisplayLocation;
    public float DisplayTime;

    private CollectibleDisplayController currentCollectible;

    public void Display(Collectible collectible)
    {
        CancelInvoke("DestroyCollectible");
        DestroyCollectible();

        if (collectible.Info.CollectedPrefab != null)
        {
            currentCollectible = GameObject.Instantiate(collectible.Info.CollectedPrefab);
            currentCollectible.Show();
            Transform cTransform = currentCollectible.transform;

            cTransform.localPosition = Vector3.zero;
            cTransform.localRotation = Quaternion.identity;
            cTransform.localScale = Vector3.one;

            cTransform.SetParent(DisplayLocation, false);
            Invoke("DestroyCollectible", DisplayTime);
        }
    }

    private void DestroyCollectible()
    {
        if (currentCollectible != null)
        {
            currentCollectible.Hide();
        }
    }
}
