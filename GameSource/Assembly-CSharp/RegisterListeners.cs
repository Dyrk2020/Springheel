using UnityEngine;

public class RegisterListeners : MonoBehaviour
{
	public Camera CameraToFollow;

	private void Awake()
	{
		AkSoundEngine.RegisterGameObj(base.gameObject);
	}

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		Camera camera = null;
		if (ZoomCamera.CurrentZoomCamera != null)
		{
			camera = ZoomCamera.CurrentZoomCamera;
		}
		else if (Camera.main != null)
		{
			camera = Camera.main;
		}
		if (camera != null)
		{
			base.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, 0f);
		}
	}
}
