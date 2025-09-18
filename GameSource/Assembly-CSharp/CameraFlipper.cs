using UnityEngine;

public class CameraFlipper : MonoBehaviour
{
	public Camera flippedCamera;

	private Modifiers.CameraFlipModes currentCameraFlipMode;

	private Matrix4x4 flipperMatrix;

	private void Awake()
	{
		flipperMatrix = Matrix4x4.Scale(Vector3.one);
	}

	private void OnPreCull()
	{
		Modifiers instance = Modifiers.GetInstance();
		if (instance.CameraFlipping != currentCameraFlipMode)
		{
			currentCameraFlipMode = instance.CameraFlipping;
			switch (currentCameraFlipMode)
			{
			case Modifiers.CameraFlipModes.None:
				flippedCamera.ResetProjectionMatrix();
				break;
			case Modifiers.CameraFlipModes.FlipX:
				flipperMatrix = Matrix4x4.Scale(new Vector3(-1f, 1f, 1f));
				break;
			case Modifiers.CameraFlipModes.FlipY:
				flipperMatrix = Matrix4x4.Scale(new Vector3(1f, -1f, 1f));
				break;
			case Modifiers.CameraFlipModes.FlipXY:
				flipperMatrix = Matrix4x4.Scale(new Vector3(-1f, -1f, 1f));
				break;
			}
		}
		if (currentCameraFlipMode != Modifiers.CameraFlipModes.None)
		{
			flippedCamera.projectionMatrix = Matrix4x4.Perspective(flippedCamera.fieldOfView, flippedCamera.aspect, flippedCamera.nearClipPlane, flippedCamera.farClipPlane) * flipperMatrix;
		}
	}

	private void OnPreRender()
	{
		if (Modifiers.GetInstance().CameraFlipping != Modifiers.CameraFlipModes.None)
		{
			GL.invertCulling = true;
		}
	}

	private void OnPostRender()
	{
		if (Modifiers.GetInstance().CameraFlipping != Modifiers.CameraFlipModes.None)
		{
			GL.invertCulling = false;
		}
	}
}
