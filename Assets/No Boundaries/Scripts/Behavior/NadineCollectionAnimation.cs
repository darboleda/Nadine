using UnityEngine;
using System.Collections;

public class NadineCollectionAnimation : MonoBehaviour {

    public Transform DisplayLocation;
    public float DisplayTime;

    public AudioSource Audio;

    private Collectible currentCollectible;

    public void Display(Collectible collectible)
    {
        CancelInvoke("DestroyCollectible");
        DestroyCollectible();

        if (collectible.CollectionSound != null)
        {
            Audio.clip = collectible.CollectionSound;
            Audio.Play();
        }

        collectible.DisableCollisions();
        Transform cTransform = collectible.transform;

        cTransform.localPosition = Vector3.zero;
        cTransform.localRotation = Quaternion.identity;
        cTransform.localScale = Vector3.one;

        cTransform.SetParent(DisplayLocation, false);
        Invoke("DestroyCollectible", DisplayTime);
        collectible.StartCollectionAnimation(DisplayTime);
        currentCollectible = collectible;
    }

    private void DestroyCollectible()
    {
        if (currentCollectible != null)
        {
            currentCollectible.DestroySelf();
        }
    }
}
