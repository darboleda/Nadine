using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OctorockTest : MonoBehaviour, IEventListener {


    [System.Serializable]
    public struct ShotInfo
    {
        public Bullet BulletOverride;

        [Range(0, 360)]
        public float Angle;
        public float Speed;

        public int Damage;
        public Knockback Knockback;
        public List<DamageDescriptor> DamageDescriptors;
    }

    public List<DamageDescriptor> DefaultDamageDescriptors;
    public WalkerPhysics Physics;
    public Transform BulletOrigin;

    public Bullet DefaultBullet;
    public ShotInfo[] Shots;

    public float MoveDistanceMax;
    public AnimationCurve MoveProbability;
    public AnimationCurve MoveDistanceProbability;
	
    public void OnEnable()
    {
        StartCoroutine(Act());
    }

    private IEnumerator Act()
    {
        while (true)
        {
            yield return StartCoroutine(Move());
            yield return StartCoroutine(Shoot());
        }
    }

    #region IEventListener implementation

    void IEventListener.Event(string eventName, params object[] args)
    {
        lastEvent = eventName;
    }

    #endregion

    private string lastEvent;
    private Coroutine WaitForEvent(string eventName)
    {
        return StartCoroutine(WaitForSecondsOrEventHelper(eventName));
    }

    private Coroutine WaitForSecondsOrEvent(string eventName, float seconds)
    {
        return StartCoroutine(WaitForSecondsOrEventHelper(eventName, seconds));
    }
    
    private IEnumerator WaitForSecondsOrEventHelper(string eventName, float seconds = float.PositiveInfinity)
    {
        do
        {
            yield return null;
            seconds -= Time.deltaTime;
        } while (lastEvent != eventName && seconds > 0);
        
        lastEvent = null;
    }

    public void TakeHit(HitBox.Properties hit)
    {
        StopAllCoroutines();
        StartCoroutine(DoHit(hit.Damage));
    }


    public int Health;
    private IEnumerator DoHit(int damage)
    {
        Health -= damage;

        if (Health > 0)
        {
            GetComponent<Animator>().SetTrigger("Damaged");
            yield return WaitForEvent("Damaged Done");
            StartCoroutine(Act());
        }
        else
        {
            GetComponent<Animator>().SetTrigger("Dying");
            yield return WaitForEvent("Dying Done");
            GameObject.Destroy(gameObject);
        }
    }

    private IEnumerator Move()
    {
        do
        {

            float direction = Random.Range(0, 360f).SnapToAngle(90);

            if (direction != Physics.Direction)
            {
                yield return new WaitForSeconds(Random.Range(0f, 0.5f));
            }

            Physics.Direction = direction;
            Physics.Speed = 1;

            yield return WaitForSecondsOrEvent("Collided", MoveDistanceProbability.Evaluate(Random.Range(0, 1f) * MoveDistanceMax));

            Physics.Speed = 0;
        
        } while (MoveProbability.Evaluate(Random.Range(0, 1f)) > 0.5f);
    }


    private IEnumerator Shoot()
    {
        GetComponent<Animator>().SetTrigger("Shoot");
        yield return WaitForEvent("Preshot End");

        ShotInfo shot = Shots[(int)(Physics.Direction / (360 / Shots.Length)) % Shots.Length];

        Bullet clone = GameState.Instantiate(shot.BulletOverride ?? DefaultBullet);
        clone.transform.position = BulletOrigin.position;
        HitBox.Properties properties = clone.HitBox.HitProperties;

        properties.Damage = shot.Damage;
        properties.Knockback = shot.Knockback;
        properties.Description = new List<DamageDescriptor>(DefaultDamageDescriptors.Concat(shot.DamageDescriptors));

        clone.HitBox.HitProperties = properties;

        clone.Fire(shot.Speed * new Vector2(Mathf.Cos(shot.Angle * Mathf.Deg2Rad), Mathf.Sin(shot.Angle * Mathf.Deg2Rad)));

        yield return WaitForEvent("Aftershot End");
    }
}
