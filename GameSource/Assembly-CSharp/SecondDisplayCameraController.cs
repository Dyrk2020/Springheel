using GameEvent;
using UnityEngine;

public class SecondDisplayCameraController : MonoBehaviour, IGameEventListener, InputReceiver
{
	public static SecondDisplayCameraController instance;

	protected Camera syncedCamera;

	public Camera SecondaryCamera;

	public static int playerMaskControlsCamera = -1;

	private bool ModiferHold;

	private bool RightShoulderHold;

	private bool LeftShoulderHold;

	private float SecondarySprint = 1f;

	private float SecondaryExtraSmoothness;

	private float moveSpeedModifier = 20f;

	private float zoomSpeedModifier = 3.5f;

	private float SecondaryCameraFriction = 0.982f;

	private Vector3 SecondaryCameraVelocity;

	private void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
		}
	}

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		ChangeListener(adding: true);
		Controller.AddGlobalReceiver(this);
	}

	protected void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<ControllerControlsCamera>(this, adding);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
		Controller.RemoveGlobalReceiver(this);
	}

	private void FixedUpdate()
	{
		if (SecondaryCamera != null)
		{
			Vector3 vector = SecondaryCameraVelocity * Time.deltaTime * (ModiferHold ? 2f : 1f) * GameSettings.GetInstance().SecondaryCameraSpeedVsFOV.Evaluate(SecondaryCamera.fieldOfView);
			vector.z = 0f;
			SecondaryCamera.transform.position += vector;
			SecondaryCamera.fieldOfView += SecondaryCameraVelocity.z * Time.deltaTime * GameSettings.GetInstance().SecondaryCameraSpeedVsFOV.Evaluate(SecondaryCamera.fieldOfView);
			SecondaryCameraVelocity *= SecondaryCameraFriction + Mathf.Lerp(0f, 1f - SecondaryCameraFriction, SecondaryExtraSmoothness);
			ResetManualValues();
		}
	}

	private void SetupSecondaryCamera(ZoomCamera zoomCamera)
	{
		GameObject gameObject = new GameObject("SecondaryCamera");
		SecondaryCamera = gameObject.AddComponent<Camera>();
		SyncToCamera(zoomCamera.useCamera);
		if (Application.isEditor)
		{
			SecondaryCamera.targetDisplay = 1;
		}
		else if (Display.displays.Length > 1)
		{
			Display.displays[1].Activate();
			SecondaryCamera.targetDisplay = 1;
		}
		else
		{
			Debug.Log("No Secondary Display, deleting secondary displaycamera.");
			Object.Destroy(gameObject);
			SecondaryCamera = null;
		}
		Object.DontDestroyOnLoad(SecondaryCamera.gameObject);
	}

	public void SyncToCamera(Camera camera)
	{
		SecondaryCamera.transform.position = camera.transform.position;
		SecondaryCamera.fieldOfView = camera.fieldOfView;
		SecondaryCamera.farClipPlane = camera.farClipPlane;
		SecondaryCamera.nearClipPlane = camera.nearClipPlane;
		SecondaryCamera.useOcclusionCulling = camera.useOcclusionCulling;
		SecondaryCamera.cullingMask = camera.cullingMask;
		SecondaryCamera.depth = camera.depth;
		syncedCamera = camera;
	}

	private void ResetManualValues()
	{
		SecondaryExtraSmoothness = 0f;
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (e.PlayerBitMask != playerMaskControlsCamera)
		{
			return;
		}
		e.Consume();
		if (e.Key == InputEvent.InputKey.Sprint && e.Changed)
		{
			return;
		}
		switch (e.Key)
		{
		case InputEvent.InputKey.LeftTrigger:
			SecondaryExtraSmoothness = Mathf.Lerp(0f, 1f, (e.Valuef - 0.25f) / 0.75f);
			break;
		case InputEvent.InputKey.RightTrigger:
			SecondarySprint = 1f + Mathf.Lerp(0f, 2f, (e.Valuef - 0.25f) / 0.75f);
			break;
		}
		if (e.Changed)
		{
			switch (e.Key)
			{
			case InputEvent.InputKey.Sprint:
				ModiferHold = e.Valueb;
				break;
			case InputEvent.InputKey.RotateLeft:
				LeftShoulderHold = e.Valueb;
				CheckShoulderButtonHold();
				break;
			case InputEvent.InputKey.RotateRight:
				RightShoulderHold = e.Valueb;
				CheckShoulderButtonHold();
				break;
			}
		}
		if (Mathf.Abs(e.Valuef) < 0.2f)
		{
			return;
		}
		switch (e.Key)
		{
		case InputEvent.InputKey.Up:
			SecondaryCameraVelocity.y += e.Valuef * Time.fixedDeltaTime * moveSpeedModifier * SecondarySprint;
			break;
		case InputEvent.InputKey.Down:
			SecondaryCameraVelocity.y -= e.Valuef * Time.fixedDeltaTime * moveSpeedModifier * SecondarySprint;
			break;
		case InputEvent.InputKey.Left:
			SecondaryCameraVelocity.x -= e.Valuef * Time.fixedDeltaTime * moveSpeedModifier * SecondarySprint;
			break;
		case InputEvent.InputKey.Right:
			SecondaryCameraVelocity.x += e.Valuef * Time.fixedDeltaTime * moveSpeedModifier * SecondarySprint;
			break;
		case InputEvent.InputKey.Up2:
			SecondaryCameraVelocity.z -= e.Valuef * Time.fixedDeltaTime * zoomSpeedModifier * SecondarySprint;
			break;
		case InputEvent.InputKey.Down2:
			SecondaryCameraVelocity.z += e.Valuef * Time.fixedDeltaTime * zoomSpeedModifier * SecondarySprint;
			break;
		case InputEvent.InputKey.DpadUp:
			SecondaryCameraVelocity.y -= e.Valuef * Time.fixedDeltaTime * moveSpeedModifier * SecondarySprint;
			if (e.Valueb && !ModiferHold)
			{
				moveSpeedModifier *= 1.01f;
				UserMessageManager.Instance.UserMessage("Camera Move Speed:" + moveSpeedModifier.ToString("F1"), 0.4f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: false);
			}
			break;
		case InputEvent.InputKey.DpadDown:
			SecondaryCameraVelocity.y += e.Valuef * Time.fixedDeltaTime * moveSpeedModifier * SecondarySprint;
			if (e.Valueb && !ModiferHold)
			{
				moveSpeedModifier *= 0.99f;
				UserMessageManager.Instance.UserMessage("Camera Movespeed:" + moveSpeedModifier.ToString("F1"), 0.4f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: false);
			}
			break;
		case InputEvent.InputKey.DpadLeft:
			SecondaryCameraVelocity.x += e.Valuef * Time.fixedDeltaTime * moveSpeedModifier * SecondarySprint;
			if (e.Valueb)
			{
				if (!ModiferHold)
				{
					zoomSpeedModifier *= 0.99f;
					UserMessageManager.Instance.UserMessage("Camera Zoomspeed:" + zoomSpeedModifier.ToString("F1"), 0.4f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: false);
				}
				else
				{
					SecondaryCameraFriction *= 0.999f;
					UserMessageManager.Instance.UserMessage("Camera Friction:" + SecondaryCameraFriction.ToString("F2"), 0.4f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: false);
				}
			}
			break;
		case InputEvent.InputKey.DpadRight:
			SecondaryCameraVelocity.x -= e.Valuef * Time.fixedDeltaTime * moveSpeedModifier * SecondarySprint;
			if (!e.Valueb)
			{
				break;
			}
			if (!ModiferHold)
			{
				zoomSpeedModifier *= 1.01f;
				UserMessageManager.Instance.UserMessage("Camera Zoomspeed:" + zoomSpeedModifier.ToString("F1"), 0.4f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: false);
				break;
			}
			SecondaryCameraFriction *= 1.001f;
			if (SecondaryCameraFriction > 1f)
			{
				SecondaryCameraFriction = 1f;
			}
			UserMessageManager.Instance.UserMessage("Camera Friction:" + SecondaryCameraFriction.ToString("F2"), 0.4f, UserMessageManager.UserMsgPriority.lo, tiedToCurrentScene: false);
			break;
		case InputEvent.InputKey.OrthoUp:
		case InputEvent.InputKey.OrthoDown:
		case InputEvent.InputKey.OrthoLeft:
		case InputEvent.InputKey.OrthoRight:
		case InputEvent.InputKey.Jump:
		case InputEvent.InputKey.Suicide:
		case InputEvent.InputKey.ChangeMode:
		case InputEvent.InputKey.Sprint:
		case InputEvent.InputKey.Inventory:
		case InputEvent.InputKey.Zoom:
		case InputEvent.InputKey.LeftTrigger:
		case InputEvent.InputKey.RightTrigger:
		case InputEvent.InputKey.RotateLeft:
		case InputEvent.InputKey.RotateRight:
		case InputEvent.InputKey.Start:
		case InputEvent.InputKey.Pause:
		case InputEvent.InputKey.Scoreboard:
		case InputEvent.InputKey.Accept:
		case InputEvent.InputKey.Back:
		case InputEvent.InputKey.NoKey:
		case InputEvent.InputKey.Left2:
		case InputEvent.InputKey.Right2:
		case InputEvent.InputKey.OrthoUp2:
		case InputEvent.InputKey.OrthoDown2:
		case InputEvent.InputKey.OrthoLeft2:
		case InputEvent.InputKey.OrthoRight2:
		case InputEvent.InputKey.VectorChanged:
		case InputEvent.InputKey.Chat:
		case InputEvent.InputKey.Esc:
			break;
		}
	}

	private void CheckShoulderButtonHold()
	{
		if (LeftShoulderHold && RightShoulderHold)
		{
			if (syncedCamera != null)
			{
				SyncToCamera(syncedCamera);
			}
			SecondaryCameraVelocity = Vector3.zero;
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (!(e.GetType() == typeof(ControllerControlsCamera)))
		{
			return;
		}
		ControllerControlsCamera controllerControlsCamera = e as ControllerControlsCamera;
		if (playerMaskControlsCamera == controllerControlsCamera.playerMaskNumber)
		{
			playerMaskControlsCamera = -1;
			Debug.Log("No Controller on Secondary Camera");
			UserMessageManager.Instance.UserMessage("Secondary Display Camera Control Off", 1f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			return;
		}
		ZoomCamera zoomCamera = Object.FindObjectOfType<ZoomCamera>();
		if (zoomCamera != null)
		{
			playerMaskControlsCamera = controllerControlsCamera.playerMaskNumber;
			Debug.Log("Controller mask " + controllerControlsCamera.playerMaskNumber + "Controls secondary camera");
			UserMessageManager.Instance.UserMessage("Controlling Secondary Display Camera", 1f, UserMessageManager.UserMsgPriority.hi, tiedToCurrentScene: false);
			SetupSecondaryCamera(zoomCamera);
		}
	}
}
