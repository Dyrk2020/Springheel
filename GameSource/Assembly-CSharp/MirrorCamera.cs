using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MirrorCamera : MonoBehaviour
{
	public Camera CameraToMirror;

	protected Camera cam;

	private void Start()
	{
		cam = GetComponent<Camera>();
	}

	protected virtual void Update()
	{
		if (CameraToMirror != null)
		{
			cam.transform.position = CameraToMirror.transform.position;
			cam.transform.rotation = CameraToMirror.transform.rotation;
			cam.transform.localScale = CameraToMirror.transform.localScale;
			cam.fieldOfView = CameraToMirror.fieldOfView;
			cam.orthographic = CameraToMirror.orthographic;
			cam.orthographicSize = CameraToMirror.orthographicSize;
		}
	}
}
