using UnityEngine;

public abstract class CameraConstraint : TimedBehavior
{
    public struct CameraInformation
    {
        private Camera camera;
        public CameraInformation(Camera camera)
        {
            this.camera = camera;
        }

        public Vector3 ViewportToWorldPoint(Vector3 position) { return camera.ViewportToWorldPoint(position); }
        public Vector3 WorldToViewportPoint(Vector3 position) { return camera.WorldToViewportPoint(position); }

        public float Aspect { get { return camera.aspect; } }
        public bool Orthographic { get { return camera.orthographic; } }
        public float OrthographicSize { get { return camera.orthographicSize; } }
        public float FieldOfView { get { return camera.fieldOfView; } }

        public bool HasCamera { get { return camera != null; } }
    }

    public virtual void Start() { }

    public abstract void Constrain(CameraInformation camera);
}
