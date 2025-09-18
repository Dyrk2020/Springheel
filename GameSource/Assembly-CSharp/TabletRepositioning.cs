using UnityEngine;

public class TabletRepositioning : MonoBehaviour
{
	private Camera UiCamera;

	private Vector2 lastScreenSize;

	private void Update()
	{
		if (UiCamera == null)
		{
			UiCamera = GetComponentInParent<Camera>();
		}
		if (UiCamera != null && ((float)Screen.width != lastScreenSize.x || (float)Screen.height != lastScreenSize.y))
		{
			Vector3 vector = UiCamera.ScreenToWorldPoint(new Vector3(0f, UiCamera.pixelHeight / 2, 250f));
			base.transform.position = new Vector3(vector.x, base.transform.position.y, base.transform.position.z);
			lastScreenSize.x = Screen.width;
			lastScreenSize.y = Screen.height;
		}
	}
}
