using UnityEngine;
using System.Collections;

public class Bauble : Collectible {

    public int Value;

    public Transform Display;
    public float Gravity;
    public float VanishTime = 4f;

    public void OnEnable()
    {
        StartCoroutine(AnimateFall());
        StartCoroutine(Disappear(VanishTime));
    }

    public IEnumerator Disappear(float vanishTime)
    {
        float flickerTime = vanishTime * 0.1f;
        yield return new WaitForSeconds(vanishTime);

        while (flickerTime > 0)
        {
            flickerTime -= Time.deltaTime;
            Display.gameObject.SetActive(!Display.gameObject.activeSelf);
            yield return null;
        }
        GameObject.Destroy(gameObject);
    }

    public IEnumerator AnimateFall()
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
}