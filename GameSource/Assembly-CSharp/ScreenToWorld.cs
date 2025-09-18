using UnityEngine;

public class ScreenToWorld : MonoBehaviour
{
	public Vector2 ScreenLocation;

	public Camera mainCamera;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(ScreenLocation.x * (float)Screen.width, ScreenLocation.y * (float)Screen.height, 0f - mainCamera.transform.position.z));
	}
}
