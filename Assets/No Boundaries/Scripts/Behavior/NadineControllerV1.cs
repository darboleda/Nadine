using UnityEngine;
using System.Collections;

[System.Serializable]
public struct NadineMovementConfig
{
    public float WalkSpeed;
    public float WalkAcceleration;
    public float WalkFriction;

    public Vector3 North;
    public Vector3 East;

    public Vector3 TransformMovementDelta(float direction, float speed)
    {
        float radDirection = direction * Mathf.Deg2Rad;
        return speed * (North * Mathf.Sin(radDirection) + East * Mathf.Cos(radDirection));
    }
}

[System.Serializable]
public struct NadineCollisionConfig
{
    public Vector3 PositionOffset;
    public float Radius;
    public LayerMask Mask;
}

[System.Serializable]
public struct NadineAnimatorConfig
{
	public Animator Animator;
	
	public string HorizontalDirectionParamName;
	public string VerticalDirectionParamName;
	public string MovingParamName;
	public string DamagedTriggerName;
	
	public string ShieldAttackTriggerName;
	public string ShieldTypeIdParamName;
	
	public void SetDirection(float radians)
	{
		float val = Mathf.Cos(radians);
		if (Mathf.Abs(val) < 0.01) val = 0;
		Animator.SetFloat(HorizontalDirectionParamName, val);
		val = Mathf.Sin(radians);
		if (Mathf.Abs(val) < 0.01) val = 0;
		Animator.SetFloat(VerticalDirectionParamName, val);
	}
	
	public void SetSpeed(float speed)
	{
		Animator.SetBool(MovingParamName, speed > 0);
	}
	
	public void TriggerDamaged()
	{
		Animator.SetTrigger(DamagedTriggerName);
	}
	
	public void StartShieldAttack(int shieldId)
	{
		Animator.SetTrigger(ShieldAttackTriggerName);
		Animator.SetInteger(ShieldTypeIdParamName, shieldId);
	}
}

public class NadineControllerV1 : NadineController
{
    public float Direction;
    public float Speed;

    public NadineAnimatorConfig Animator;
    public NadineMovementConfig Movement;
    public NadineDamagedConfig Damaged;
    public NadineCollisionConfig Collision;
    public AngleSnapConfig Snap;
    public PlayerHealth Health;
    public SpriteRenderer Renderer;
    public AudioSource Sound;

    public void Start()
    {
        SetAnimatorParams(Direction, Speed);
    }

    private Vector3 velocity;
    public Vector3 Velocity { get { return velocity; } }
    private Vector2 movement;
    private bool ignoreInputs;
    private bool ignoreFriction;

    public void Update()
    {
        float maxHealthAxis = Input.GetAxisRaw("Mouse ScrollWheel");
        Health.SetMaxHealth(Health.MaxHealth + (int)(maxHealthAxis * 10) * 4);

        Debug.DrawLine(transform.position, transform.position + velocity / Movement.WalkSpeed, Color.green);
        if (ignoreInputs)
        {
            return;
        }

        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");
        
        movement = new Vector2(hAxis, vAxis);
        Speed = (movement.sqrMagnitude < 0.001f ? 0 :
                 movement.sqrMagnitude > 1 ? 1 :
                 movement.magnitude);

        if (Speed > 0)
        {
            Direction = Mathf.Atan2(vAxis, hAxis) * Mathf.Rad2Deg;
        }
        
        if (Input.GetButtonDown("Fire1"))
        {
            Animator.StartShieldAttack(0);
        }

        SetAnimatorParams(Direction, Speed);
        //Renderer.sortingOrder = 13 * 2 - (int)(transform.position.y + 0.5f) * 2 - 1;
    }

    private float acc;
    public void FixedUpdate()
    {
        Vector3 acceleration = Vector3.zero;
        
        if (ignoreFriction)
        {
            velocity += acceleration * Time.deltaTime;
        }
        else
        {
            Vector3 velocityDirection = velocity.normalized;
            acceleration = Movement.TransformMovementDelta(Direction, Speed) * Movement.WalkAcceleration;
            
            float accelerationDot = Vector3.Dot(velocity, acceleration);
            acceleration = (accelerationDot > 0.01f || velocity.sqrMagnitude < 0.01f ? acceleration :
                            accelerationDot < 0.01f ? velocityDirection * -(Movement.WalkFriction + Movement.WalkAcceleration) :
                                velocityDirection * -Movement.WalkFriction);
            acc = accelerationDot;
            Vector3 newVelocity = velocity + acceleration * Time.deltaTime;
            velocity = (Vector3.Dot(velocity, newVelocity) < 0 ? Vector3.zero : newVelocity);
            velocity = (velocity.sqrMagnitude > Movement.WalkSpeed * Movement.WalkSpeed ? velocity.normalized * Movement.WalkSpeed : velocity);
        }
        
        
        Vector3 initialDelta = velocity * Time.deltaTime;
        Vector3 correctedDelta = CheckCollision2D(initialDelta);

        transform.Translate(correctedDelta);
    }

    public override void BeginAttack()
    {
        this.ignoreInputs = true;
        this.Speed = 0;
    }

    public override void EndAttack()
    {
        this.ignoreInputs = false;
    }

    public override void TakeHit(HitBox.Properties hitProperties, HurtBox hurtBox)
    {
        Health.TakeDamage(hitProperties.Damage);
        Sound.PlayOneShot(Damaged.DamagedSound);

        Vector3 velocity = Vector3.zero;
        Knockback knockback = hitProperties.Knockback;
        float speed = Damaged.KnockbackSpeed * knockback.Strength;

        if (knockback.Direction.Type == SpecialDirection.DirectionType.Origin)
        {
            Vector3 v = hitProperties.Origin.position - hurtBox.transform.position;
            v.z = 0;
            
            float angle = v.GetAngle(Movement.East, Movement.North).SnapToAngle(Damaged.KnockbackAngleSnap);
            velocity = Movement.TransformMovementDelta(angle + 180, speed);
        }

        StartCoroutine(DamageAnimation(velocity));
        Animator.TriggerDamaged();
    }

    private IEnumerator DamageAnimation(Vector3 delta)
    {
        ignoreInputs = true;
        ignoreFriction = true;

        velocity = delta;
        Vector3 friction = delta.normalized * Damaged.KnockbackFriction;
        while (true)
        {
            if (velocity.sqrMagnitude == 0) break;

            Vector3 newVel = velocity - friction * Time.deltaTime;
            if (Vector3.Dot(delta, newVel) > 0)
            {
                velocity = newVel;
            }
            else
            {
                velocity = Vector3.zero;
            }

            yield return null;
        }
        ignoreInputs = false;
        ignoreFriction = false;
    }

    private void SetAnimatorParams(float angle, float speed)
    {
        angle = Snap.Snap(angle);

        Animator.SetDirection(angle * Mathf.Deg2Rad);
        Animator.SetSpeed(speed);
    }

    private Vector3 CheckCollision2D(Vector3 delta)
    {
        Vector2 delta2D = delta;
        Vector2 position2D = transform.position + Collision.PositionOffset;
        Vector2 direction = delta2D.normalized;
        float distance = delta2D.magnitude;

        Debug.DrawLine(position2D, position2D + direction * distance * 10, Color.red);


        RaycastHit2D hit = Physics2D.CircleCast(position2D, Collision.Radius, direction, distance, Collision.Mask.value);
        if (hit.collider == null) return delta;

        if (Vector2.Dot(direction, hit.normal) >= 0) return delta;

        Vector2 delta2Dremainder = delta2D;
        delta2D = (hit.point + hit.normal * Collision.Radius) - position2D;
        delta2Dremainder -= delta2D;
        
        delta2Dremainder -= Vector2.Dot(hit.normal, delta2Dremainder) * hit.normal;

        direction = delta2Dremainder.normalized;
        distance = delta2Dremainder.magnitude;        
        position2D = position2D + delta2D;

        Debug.DrawLine(position2D, position2D + direction * distance * 10, Color.cyan);

        hit = Physics2D.CircleCast(position2D, Collision.Radius, direction, distance, Collision.Mask.value);
        
        if (hit.collider == null) return new Vector3(delta2D.x + delta2Dremainder.x, delta2D.y + delta2Dremainder.y, delta.z);
        if (Vector2.Dot(direction, hit.normal) >= 0) return new Vector3(delta2D.x + delta2Dremainder.x, delta2D.y +delta2Dremainder.y, delta.z);

        Debug.DrawLine(hit.point, hit.point + hit.normal, Color.magenta);
        delta2Dremainder = (hit.point + hit.normal * Collision.Radius) - position2D;

        delta2D = delta2D + delta2Dremainder;

        return new Vector3(delta2D.x, delta2D.y, delta.z);
    }
}
