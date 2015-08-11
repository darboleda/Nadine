using UnityEngine;
using System.Collections;

public class RandomTurret : MonoBehaviour
{
    public Vector3 North;
    public Vector3 East;

    public Vector3 TransformMovementDelta(float direction, float speed)
    {
        float radDirection = direction * Mathf.Deg2Rad;
        return speed * (North * Mathf.Sin(radDirection) + East * Mathf.Cos(radDirection));
    }

    public float AngleSnap;
    public float BulletSpeed;
    public Bullet BulletPrefab;
    public float DelayBetweenVolleys;
    public float DelayBetweenShots;
    
    public void Start()
    {
        StartCoroutine(Act());
    }

    public IEnumerator Act()
    {
        while (true)
        {
            yield return new WaitForSeconds(DelayBetweenVolleys);

            for (int i = 0; i < 360; i++)
            {
                if (i * AngleSnap >= 360) break;
                FireBullet(TransformMovementDelta(i * AngleSnap, BulletSpeed));
                yield return new WaitForSeconds(DelayBetweenShots);
            }
        }
    }

    private void FireBullet(Vector3 velocity)
    {
        Bullet bullet = GameObject.Instantiate(BulletPrefab);
        bullet.transform.position = transform.position;
        bullet.Fire(velocity, "Player");
    }

}
