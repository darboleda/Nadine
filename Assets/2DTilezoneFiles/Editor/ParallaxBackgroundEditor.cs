using UnityEngine;
using System.Collections;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(ParallaxBackground))]
public class ParallaxBackgroundEditor : Editor {

	object[] pbs;
	void CallbackFunction() {
		foreach( object o in pbs ) {
			ParallaxBackground pb = (ParallaxBackground)o;
			if( pb != null && SceneView.lastActiveSceneView != null ) {
				pb.transform.position = pb.startPosition + (Vector3)(Vector2)(SceneView.lastActiveSceneView.camera.transform.position - pb.startPosition) * ( 1 - 1 / ((pb.distance/10f)+1));
			}
		}
	}
	
	void OnEnable()
	{
		pbs = targets;
		foreach( object o in pbs ) {
			ParallaxBackground pb = (ParallaxBackground)o;
			if( pb.cam == null ) {
				pb.startPosition = pb.transform.position;
				pb.distance = pb.transform.position.z;
				pb.cam = Camera.main;

			}
			if( pb.GetComponent<Renderer>() != null )
				EditorUtility.SetSelectedWireframeHidden( pb.GetComponent<Renderer>(), true );
		}
		EditorApplication.update -= CallbackFunction;
		EditorApplication.update += CallbackFunction;
	}
	
	void OnDisable()
	{
		EditorApplication.update -= CallbackFunction;
	}

	public override void OnInspectorGUI ()
	{
		EditorGUILayout.HelpBox( "You can not modify the position of a Parallax Background using Transform, instead use Start Position.", MessageType.Info );
		DrawDefaultInspector();
		EditorGUILayout.Space();
		if( GUILayout.Button( "Preview all parallax backgrounds" ) ) {
			Selection.objects = FindObjectsOfType(typeof(ParallaxBackground));
		}
	}
}
