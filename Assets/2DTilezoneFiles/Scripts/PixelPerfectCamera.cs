using UnityEngine;
using System.Collections;

[AddComponentMenu("2D/Pixel Perfect Camera")]
public class PixelPerfectCamera : MonoBehaviour {
	
	public float pixelsPerUnit = 16;
	static float _pixelsPerUnit;
	public int zoomFactor = 1;
	public static int _zoomFactor;

	Vector3 offSet;
	
	void Start () {
		GetComponent<Camera>().orthographicSize = (float)Screen.height / 2f / pixelsPerUnit;
		_pixelsPerUnit = pixelsPerUnit;
		_zoomFactor = zoomFactor;
		if( zoomFactor > 1 )
			GetComponent<Camera>().orthographicSize /= zoomFactor;

		if( transform.parent != null )
			offSet = transform.position - transform.parent.position;
	}
	
	void LateUpdate () {
		if( transform.parent != null )
			transform.position = transform.parent.position + offSet;
		//make sure this is called after the camera has moved
		SnapCam();
	}
	
	public void SnapCam ( ) {
		Vector3 newPos = transform.position;
		newPos.x =  ((int)(newPos.x*pixelsPerUnit*zoomFactor) + (0.05f) ) / (_pixelsPerUnit*zoomFactor);
		newPos.y =  ((int)(newPos.y*pixelsPerUnit*zoomFactor) + 0.0f) / (pixelsPerUnit*zoomFactor);
		transform.position = newPos;
	}

	public static void SnapToPix ( Transform transform ) {
		Vector3 newPos = transform.position;
		newPos.x =  (int)(newPos.x*_pixelsPerUnit * _zoomFactor) / (_pixelsPerUnit * _zoomFactor);
		newPos.y =  (int)(newPos.y*_pixelsPerUnit * _zoomFactor) / (_pixelsPerUnit * _zoomFactor);
		transform.position = newPos;
	}
}
