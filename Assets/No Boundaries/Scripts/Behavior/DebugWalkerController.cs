using UnityEngine;
using System.Collections;

public class DebugWalkerController : MonoBehaviour {

	public WalkerPhysics Physics;

	public void Update()
	{
		float hAxis = Input.GetAxisRaw("Horizontal");
		float vAxis = Input.GetAxisRaw("Vertical");
		
		Vector3 movement = new Vector2(hAxis, vAxis);
		float speed = (movement.sqrMagnitude < 0.001f ? 0 :
		         	   movement.sqrMagnitude > 1 ? 1 :
		         	   movement.magnitude);

		float direction = Physics.Direction;
		if (speed > 0)
		{
			direction = Mathf.Atan2(vAxis, hAxis) * Mathf.Rad2Deg;
		}

		Physics.Speed = speed;
		Physics.Direction = direction;
	}
}
