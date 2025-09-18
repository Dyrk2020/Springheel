using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FixCameraToBoundaries : MonoBehaviour
{
	[Tooltip("Add a margin. 1 = no margin, 1.05 = 5% marging.")]
	public float margin = 1f;

	private BoxCollider2D boundaryCollider;

	public bool isCameraConfigured;

	private void Awake()
	{
		boundaryCollider = GetComponent<BoxCollider2D>();
	}

	private void Update()
	{
		if (isCameraConfigured)
		{
			base.enabled = false;
			return;
		}
		ZoomCamera zoomCamera = UnityEngine.Object.FindObjectOfType<ZoomCamera>();
		if (zoomCamera != null)
		{
			Camera component = zoomCamera.GetComponent<Camera>();
			if (component != null)
			{
				zoomCamera.enabled = false;
				AdjustCamera(component);
			}
		}
	}

	private void AdjustCamera(Camera camera)
	{
		if (camera.orthographic)
		{
			AdjustOrthographicCamera(camera);
		}
		else
		{
			AdjustPerspectiveCamera(camera);
		}
		isCameraConfigured = true;
	}

	private void AdjustOrthographicCamera(Camera camera)
	{
		if (!camera.orthographic)
		{
			Debug.LogError("Camera is not orthographic mode.", camera.gameObject);
			return;
		}
		Bounds bounds = boundaryCollider.bounds;
		camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, camera.transform.position.z);
		float num = bounds.size.x / bounds.size.y;
		float num2 = (float)Screen.width / (float)Screen.height;
		if (num2 >= num)
		{
			camera.orthographicSize = bounds.size.y / 2f * margin;
		}
		else
		{
			float num3 = bounds.size.x / num2;
			camera.orthographicSize = num3 / 2f * margin;
		}
		Debug.Log("Camera boundaries centered and fixed.");
	}

	private void AdjustPerspectiveCamera(Camera cam)
	{
		Bounds bounds = boundaryCollider.bounds;
		float num = bounds.size.x * margin;
		float a = bounds.size.y * margin / 2f / Mathf.Tan(cam.fieldOfView * 0.5f * (MathF.PI / 180f));
		float f = cam.fieldOfView * 0.5f * (MathF.PI / 180f);
		float b = num / 2f / (Mathf.Tan(f) * cam.aspect);
		float num2 = Mathf.Max(a, b);
		Vector3 position = new Vector3(bounds.center.x, bounds.center.y, bounds.center.z - num2);
		cam.transform.position = position;
		cam.transform.LookAt(bounds.center);
		Debug.Log($"Perspective camera distance updated to {num2}.");
	}
}
