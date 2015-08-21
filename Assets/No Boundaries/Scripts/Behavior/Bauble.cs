using UnityEngine;
using System.Collections;

public class Bauble : Collectible {

    public int Value;

    public Transform Display;
    public float Gravity;

    public void OnEnable()
    {
        StartCoroutine(Animate());
    }

    public IEnumerator Animate()
    {
        DisableCollisions();

        Vector3 position = new Vector3(0, Random.Range(0.5f, 1.5f), 0);
        Display.localPosition = position;
        Vector3 velocity = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        float minY = 0;
        Vector3 zPosition = transform.localPosition;

        while (Display.localPosition.y > minY)
        {
            yield return null;

            velocity.y += -Gravity * Time.deltaTime;
            position.x += velocity.x * Time.deltaTime;
            position.y += velocity.y * Time.deltaTime;
            zPosition.y += velocity.z * Time.deltaTime;
            Display.localPosition = position;
        }

        position.y = minY;
        Display.localPosition = position;

        EnableCollisions();

        velocity.y = -velocity.y * 0.5f;
        velocity.x *= 0.85f;
        velocity.z *= 0.85f;

        while (true)
        {
            yield return null;

            velocity.y += -Gravity * Time.deltaTime;
            position.x += velocity.x * Time.deltaTime;
            position.y += velocity.y * Time.deltaTime;
            zPosition.y += velocity.z * Time.deltaTime;

            if (position.y <= minY)
            {
                velocity.y = -velocity.y * 0.5f;
                velocity.x *= 0.85f;
                velocity.z *= 0.85f;
                position.y = minY;
            }

            Display.localPosition = position;
            transform.localPosition = zPosition;
        }
    }

    public override void StartCollectionAnimation(float expectedDisplayTime)
    {
        Display.localPosition = Vector3.zero;
        var sorter = GetComponent<AutoSpriteSorter>();
        if (sorter != null)
        {
            sorter.Offset.y = -1000;
        }

        StopAllCoroutines();
        StartCoroutine(AnimateCollection());
    }

    private IEnumerator AnimateCollection()
    {
        Vector3 position = Display.localPosition;

        float x = 0;
        float xVel = 1f;
        float xFriction = 0.5f;
        while (xVel > 0)
        {

            x += xVel * Time.deltaTime;
            position.y = 0.2f * Mathf.Cos(x * Mathf.PI * 6);
            Display.localPosition = position;

            xVel -= xFriction * Time.deltaTime;

            yield return null;
        }
    }

    public override void DestroySelf()
    {
        StartCoroutine(Shrink(0.1f));
    }

    private IEnumerator Shrink(float time)
    {
        Vector3 scale = Display.localScale;
        Vector3 scaleVel = -scale / time;

        while (time > 0)
        {
            time -= Time.deltaTime;
            scale += scaleVel * Time.deltaTime;
            Display.localScale = scale;

            yield return null;
        }

        base.DestroySelf();
    }
}