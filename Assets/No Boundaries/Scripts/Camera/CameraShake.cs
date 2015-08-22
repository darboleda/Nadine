using UnityEngine;
using System.Collections;

public class CameraShake : CameraConstraint {

    public void Jolt(Vector3 jolt, float springStrength, float springDamping)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateShake(jolt, springStrength, springDamping));
    }

    public override void Constrain(CameraInformation camera)
    {
        transform.localPosition += offset;
    }


    public float jolt, strength, damping;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float angle = Random.Range(0, 2 * Mathf.PI);
            //Jolt(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * jolt, strength, damping);
        }
    }
	
    Vector3 offset;
    private IEnumerator AnimateShake(Vector3 jolt, float springStrength, float springDamping)
    {
        Vector3 velocity = jolt;

        while (velocity.sqrMagnitude > 0.01f || offset.sqrMagnitude > 0.01f)
        {
            velocity += (-offset * springStrength - velocity * springDamping) * Time.deltaTime;
            offset += velocity * Time.deltaTime;
            yield return null;
        }

        offset = Vector3.zero;
    }
}
