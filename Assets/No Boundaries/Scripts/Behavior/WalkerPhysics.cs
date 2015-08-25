using UnityEngine;
using System.Collections;

public class WalkerPhysics : MonoBehaviour {

	[System.Serializable]
	public struct WorldCoordinates
	{
		public Vector3 North;
		public Vector3 East;
		
		public Vector3 Transform(float direction, float speed)
		{
			float radDirection = direction * Mathf.Deg2Rad;
			return speed * (North * Mathf.Sin(radDirection) + East * Mathf.Cos(radDirection));
		}
	}


	[System.Serializable]
	public struct CollisionConfig
	{
		public Vector3 PositionOffset;
		public float Radius;
		public LayerMask Mask;
	}

	public CollisionConfig Collision;
	public WorldCoordinates World;
    
    public float Direction;
    public float Speed;

	public float AccelerationMultiplier = 50;
	public float MaxSpeedMultiplier = 5;
	public float FrictionMultiplier = 50;

	public Vector3 velocity;

	public void SetVelocity(float angle, float speed)
	{
		velocity = World.Transform(angle, speed);
	}
    
    public void FixedUpdate()
    {
        Vector3 velocityDirection = velocity.normalized;
		float calculatedSpeed = Mathf.Clamp(Speed, -1, 1);
        
		float targetSpeed = calculatedSpeed * MaxSpeedMultiplier;
		Vector3 targetVelocity = World.Transform(Direction, targetSpeed);
		Vector3 accelerationDirection = (targetVelocity - velocity).normalized;


		float accelerationAngle = accelerationDirection.GetAngle(velocity, Vector3.forward);

		Vector3 acceleration;
		if (targetVelocity.sqrMagnitude < 0.01f)
		{
			acceleration = -velocityDirection * FrictionMultiplier;
		}
		else if (velocity.sqrMagnitude < 0.01f)
		{
			acceleration = accelerationDirection * AccelerationMultiplier;
		}
		else if (Mathf.Abs(accelerationAngle) > 90)
		{
			acceleration = (accelerationDirection * AccelerationMultiplier) - (velocityDirection * FrictionMultiplier);
		}
		else
		{
			acceleration = accelerationDirection * AccelerationMultiplier;
		}

		Vector3 newVelocity = velocity + acceleration * Time.deltaTime;
		if (Vector3.Dot(newVelocity - targetVelocity, velocity - targetVelocity) < 0)
		{
			newVelocity = targetVelocity;
		}
		velocity = newVelocity;

        
        Vector3 initialDelta = velocity * Time.deltaTime;
        bool collided;
        Vector3 correctedDelta = CheckCollision2D(initialDelta, out collided);

        if (collided)
        {
            EventRebroadcaster broadcaster = GetComponent<EventRebroadcaster>();
            if (broadcaster != null) broadcaster.BroadcastEvent("Collided");
        }

        transform.Translate(correctedDelta);
    }

	private Vector3 CheckCollision2D(Vector3 delta, out bool collided)
    {
        Vector2 delta2D = delta;
        Vector2 position2D = transform.position + Collision.PositionOffset;
        Vector2 direction = delta2D.normalized;
        float distance = delta2D.magnitude;

        collided = false;

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

        if (distance < 0.01f) collided = true;

        Debug.DrawLine(position2D, position2D + direction * distance * 10, Color.cyan);

        hit = Physics2D.CircleCast(position2D, Collision.Radius, direction, distance, Collision.Mask.value);
        
        if (hit.collider == null) return new Vector3(delta2D.x + delta2Dremainder.x, delta2D.y + delta2Dremainder.y, delta.z);
        if (Vector2.Dot(direction, hit.normal) >= 0) return new Vector3(delta2D.x + delta2Dremainder.x, delta2D.y +delta2Dremainder.y, delta.z);

        collided = true;

        Debug.DrawLine(hit.point, hit.point + hit.normal, Color.magenta);
        delta2Dremainder = (hit.point + hit.normal * Collision.Radius) - position2D;

        delta2D = delta2D + delta2Dremainder;

        return new Vector3(delta2D.x, delta2D.y, delta.z);
    }
}
