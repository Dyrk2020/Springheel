using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiControllerUIManager : MonoBehaviour
{
	public enum ControllerType
	{
		KeyboardAndMouse,
		KeyboardAndMouseAlt,
		Xbox,
		DualShock4,
		Switch,
		SingleJoyconL,
		SingleJoyconR
	}

	private static MultiControllerUIManager instance;

	public ControllerType currentControllerTypeRegular;

	public ControllerType currentControllerTypeJoinIndicator;

	public float switchDelaySeconds = 1f;

	public bool showAltBinding;

	public Sprite KeyboardKeySprite;

	public Sprite KeyboardSpacebarSprite;

	public Sprite KeyboardAltSprite;

	public Sprite KeyboardReturnSprite;

	public Sprite KeyboardShiftSprite;

	public Sprite MouseLeftButton;

	public Sprite MouseRightButton;

	public Sprite XboxButtonASprite;

	public Sprite XboxButtonBSprite;

	public Sprite XboxButtonXSprite;

	public Sprite XboxButtonYSprite;

	public Sprite XboxButtonLBSprite;

	public Sprite XboxButtonRBSprite;

	public Sprite PS4ButtonXSprite;

	public Sprite PS4ButtonCircleSprite;

	public Sprite PS4ButtonSquareSprite;

	public Sprite PS4ButtonTriangleSprite;

	public Sprite PS4ButtonL1Sprite;

	public Sprite PS4ButtonR1Sprite;

	public Sprite SwitchButtonASprite;

	public Sprite SwitchButtonBSprite;

	public Sprite SwitchButtonXSprite;

	public Sprite SwitchButtonYSprite;

	public Sprite SwitchTopButtonSprite;

	public Sprite SwitchBottomButtonSprite;

	public Sprite SwitchLeftButtonSprite;

	public Sprite SwitchRightButtonSprite;

	public Sprite SwitchButtonLSprite;

	public Sprite SwitchButtonRSprite;

	public Sprite SwitchButtonSLSprite;

	public Sprite SwitchButtonSRSprite;

	private float switchTimer;

	public float fadeDuration = 0.1f;

	private static bool inTreehouse;

	private List<ControllerType> allowedControllerTypeCache = new List<ControllerType>(32);

	private List<ControllerType> hypotheticalControllersForPlatform = new List<ControllerType>
	{
		ControllerType.Xbox,
		ControllerType.DualShock4
	};

	public static MultiControllerUIManager Instance
	{
		get
		{
			if (instance == null)
			{
				Debug.LogError("MultiControllerUIManager instance is null!");
			}
			return instance;
		}
	}

	private ControllerType DefaultPlatformController => ControllerType.KeyboardAndMouse;

	public bool PlatformHasKeyboard => true;

	public bool ShouldShowAllConnectedControllers => inTreehouse;

	private bool KeyboardAlreadyAssigned => GameState.GetInstance().Keyboard.GetControlMask() != 0;

	private bool HaveFreeNonKeyboardControllers
	{
		get
		{
			if (!ControllerOfTypeFree(ControllerType.Xbox) && !ControllerOfTypeFree(ControllerType.DualShock4) && !ControllerOfTypeFree(ControllerType.Switch) && !ControllerOfTypeFree(ControllerType.SingleJoyconL))
			{
				return ControllerOfTypeFree(ControllerType.SingleJoyconR);
			}
			return true;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			SceneManager.activeSceneChanged += onSceneChanged;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		_ = instance == this;
	}

	private static void onSceneChanged(Scene scene, Scene newScene)
	{
		inTreehouse = newScene.name.Equals("TreeHouseLobby");
	}

	private void Update()
	{
		if (switchTimer > switchDelaySeconds)
		{
			switchTimer %= switchDelaySeconds;
			currentControllerTypeRegular = GetNextControllerType(currentControllerTypeRegular, isJoinIndicator: false);
			currentControllerTypeJoinIndicator = GetNextControllerType(currentControllerTypeJoinIndicator, isJoinIndicator: true);
		}
		switchTimer += Time.unscaledDeltaTime;
	}

	private List<ControllerType> GetAllowedControllerTypes(bool isJoinIndicator)
	{
		bool inUse = !ShouldShowAllConnectedControllers;
		allowedControllerTypeCache.Clear();
		List<ControllerType> list = allowedControllerTypeCache;
		if (PlatformHasKeyboard)
		{
			if (isJoinIndicator)
			{
				if (ControllerOfTypeFree(ControllerType.KeyboardAndMouse))
				{
					list.Add(ControllerType.KeyboardAndMouse);
					if (showAltBinding)
					{
						list.Add(ControllerType.KeyboardAndMouseAlt);
					}
				}
			}
			else if (ControllerOfTypeConnected(inUse, ControllerType.KeyboardAndMouse))
			{
				list.Add(ControllerType.KeyboardAndMouse);
				if (showAltBinding)
				{
					list.Add(ControllerType.KeyboardAndMouseAlt);
				}
			}
		}
		if (isJoinIndicator)
		{
			if (ControllerOfTypeFree(ControllerType.Xbox))
			{
				list.Add(ControllerType.Xbox);
			}
			if (ControllerOfTypeFree(ControllerType.DualShock4))
			{
				list.Add(ControllerType.DualShock4);
			}
			if (ControllerOfTypeFree(ControllerType.Switch))
			{
				list.Add(ControllerType.Switch);
			}
			if (ControllerOfTypeFree(ControllerType.SingleJoyconL))
			{
				list.Add(ControllerType.SingleJoyconL);
			}
			if (ControllerOfTypeFree(ControllerType.SingleJoyconR))
			{
				list.Add(ControllerType.SingleJoyconR);
			}
		}
		else
		{
			if (ControllerOfTypeConnected(inUse, ControllerType.Xbox))
			{
				list.Add(ControllerType.Xbox);
			}
			if (ControllerOfTypeConnected(inUse, ControllerType.DualShock4))
			{
				list.Add(ControllerType.DualShock4);
			}
			if (ControllerOfTypeConnected(inUse, ControllerType.Switch))
			{
				list.Add(ControllerType.Switch);
			}
			if (ControllerOfTypeConnected(inUse, ControllerType.SingleJoyconL))
			{
				list.Add(ControllerType.SingleJoyconL);
			}
			if (ControllerOfTypeConnected(inUse, ControllerType.SingleJoyconR))
			{
				list.Add(ControllerType.SingleJoyconR);
			}
		}
		return list;
	}

	private ControllerType GetNextControllerType(ControllerType type, bool isJoinIndicator)
	{
		List<ControllerType> allowedControllerTypes = GetAllowedControllerTypes(isJoinIndicator);
		if (isJoinIndicator && allowedControllerTypes.Count == 0)
		{
			allowedControllerTypes = hypotheticalControllersForPlatform;
		}
		if (allowedControllerTypes.Count == 0)
		{
			return DefaultPlatformController;
		}
		int num = allowedControllerTypes.IndexOf(type);
		if (num == -1)
		{
			return allowedControllerTypes[0];
		}
		int index = (num + 1) % allowedControllerTypes.Count;
		return allowedControllerTypes[index];
	}

	private bool KeyboardAndMouseConnected(bool inUse)
	{
		if (PlatformHasKeyboard)
		{
			if (inUse)
			{
				return GameState.GetInstance().Keyboard.GetControlMask() != 0;
			}
			return true;
		}
		return false;
	}

	private bool ControllerOfTypeConnected(bool inUse, ControllerType controllerType)
	{
		if (controllerType == ControllerType.KeyboardAndMouseAlt)
		{
			controllerType = ControllerType.KeyboardAndMouse;
		}
		if (controllerType == ControllerType.KeyboardAndMouse)
		{
			return IsControllerConnected(GameState.GetInstance().Keyboard, controllerType, inUse);
		}
		foreach (Controller controller in GameState.GetInstance().Controllers)
		{
			if (IsControllerConnected(controller, controllerType, inUse))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsControllerConnected(Controller controller, ControllerType controllerType, bool inUse)
	{
		if (GetControllerType(controller) == controllerType && (!inUse || controller.GetControlMask() != 0))
		{
			InControlController inControlController = controller as InControlController;
			if (inControlController != null)
			{
				InputDevice inputDevice = inControlController.GetInputDevice();
				if (inputDevice == null || inputDevice.IsAttached)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ControllerOfTypeFree(ControllerType controllerType)
	{
		if (controllerType == ControllerType.KeyboardAndMouseAlt)
		{
			controllerType = ControllerType.KeyboardAndMouse;
		}
		if (controllerType == ControllerType.KeyboardAndMouse)
		{
			return IsControllerFree(GameState.GetInstance().Keyboard, ControllerType.KeyboardAndMouse);
		}
		foreach (Controller controller in GameState.GetInstance().Controllers)
		{
			if (IsControllerFree(controller, controllerType))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsControllerFree(Controller controller, ControllerType controllerType)
	{
		if (controller.GetControlMask() != 0 || GetControllerType(controller) != controllerType)
		{
			return false;
		}
		InControlController inControlController = controller as InControlController;
		if (!(inControlController == null))
		{
			InputDevice inputDevice = inControlController.GetInputDevice();
			if (inputDevice == null || inputDevice.IsAttached)
			{
				return true;
			}
		}
		return false;
	}

	private bool XboxControllerConnected(bool inUse)
	{
		return ControllerOfTypeConnected(inUse, ControllerType.Xbox);
	}

	private bool DualShock4ControllerConnected(bool inUse)
	{
		return ControllerOfTypeConnected(inUse, ControllerType.DualShock4);
	}

	private bool SwitchControllerConnected(bool inUse)
	{
		return ControllerOfTypeConnected(inUse, ControllerType.Switch);
	}

	private bool SwitchJoyconLConnected(bool inUse)
	{
		return ControllerOfTypeConnected(inUse, ControllerType.SingleJoyconL);
	}

	private bool SwitchJoyconRConnected(bool inUse)
	{
		return ControllerOfTypeConnected(inUse, ControllerType.SingleJoyconR);
	}

	private bool AnyControllerConnected(bool inUseOnly)
	{
		if (!KeyboardAndMouseConnected(inUseOnly) && !XboxControllerConnected(inUseOnly) && !DualShock4ControllerConnected(inUseOnly) && !SwitchControllerConnected(inUseOnly) && !SwitchJoyconLConnected(inUseOnly))
		{
			return SwitchJoyconRConnected(inUseOnly);
		}
		return true;
	}

	private bool ButtonNeedsUpdate(MultiControllerButton button, ControllerType controllerType)
	{
		if (button.firstUpdate && button.Hidden == button.lastHiddenState && button.lastUpdateControllerType == controllerType)
		{
			return button.lastUpdateInputKey != button.inputKey;
		}
		return true;
	}

	public void UpdateButton(MultiControllerButton button)
	{
		ControllerType controllerType = (button.isJoinIndicator ? currentControllerTypeJoinIndicator : currentControllerTypeRegular);
		if (button.forceControllerType)
		{
			button.canvasGroup.alpha = 1f;
			controllerType = button.forcedControllerType;
		}
		else
		{
			float num = switchDelaySeconds * fadeDuration;
			if (switchTimer < num)
			{
				button.canvasGroup.alpha = Mathf.Clamp01(switchTimer / num);
			}
			else
			{
				float num2 = switchDelaySeconds - num;
				if (switchTimer >= num2)
				{
					button.canvasGroup.alpha = 1f - Mathf.Clamp01((switchTimer - num2) / num);
				}
				else
				{
					button.canvasGroup.alpha = 1f;
				}
			}
		}
		if (!ButtonNeedsUpdate(button, controllerType))
		{
			return;
		}
		button.firstUpdate = true;
		button.lastUpdateInputKey = button.inputKey;
		button.lastHiddenState = button.Hidden;
		button.lastUpdateControllerType = controllerType;
		KeyboardInput keyboard = GameState.GetInstance().Keyboard;
		bool flag = false;
		switch (controllerType)
		{
		case ControllerType.KeyboardAndMouseAlt:
			flag = true;
			goto case ControllerType.KeyboardAndMouse;
		case ControllerType.KeyboardAndMouse:
		{
			bool flag2 = false;
			if (button.preferMouseButtons)
			{
				switch (button.inputKey)
				{
				case InputEvent.InputKey.Accept:
					button.buttonText.text = "";
					button.buttonText.enabled = false;
					button.SetImageSprite(MouseLeftButton);
					flag2 = true;
					break;
				case InputEvent.InputKey.Suicide:
				case InputEvent.InputKey.Back:
					button.buttonText.text = "";
					button.buttonText.enabled = false;
					button.SetImageSprite(MouseRightButton);
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				break;
			}
			KeyCode keyCode = KeyCode.None;
			if (flag)
			{
				KeyCode keyCode2 = keyboard.GetAltKeyBinding(button.inputKey) ?? KeyCode.None;
				keyCode = ((keyCode2 == KeyCode.None) ? (keyboard.GetKeyBinding(button.inputKey) ?? KeyCode.None) : keyCode2);
			}
			else
			{
				keyCode = keyboard.GetKeyBinding(button.inputKey) ?? KeyCode.None;
			}
			switch (keyCode)
			{
			default:
			{
				string text = keyCode.ToString();
				if (text.Length > 2)
				{
					text = "???";
				}
				button.buttonText.text = text;
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			}
			case KeyCode.Space:
				button.buttonText.text = "";
				button.buttonText.enabled = false;
				button.SetImageSprite(KeyboardSpacebarSprite);
				break;
			case KeyCode.RightShift:
			case KeyCode.LeftShift:
				button.buttonText.text = "⇧";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardShiftSprite);
				break;
			case KeyCode.Tab:
				button.buttonText.text = "↹";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardShiftSprite);
				break;
			case KeyCode.RightAlt:
			case KeyCode.LeftAlt:
				button.buttonText.text = "Alt";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardAltSprite);
				break;
			case KeyCode.RightControl:
			case KeyCode.LeftControl:
				button.buttonText.text = "Ctrl";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardAltSprite);
				break;
			case KeyCode.RightMeta:
			case KeyCode.LeftMeta:
			case KeyCode.LeftWindows:
			case KeyCode.RightWindows:
				button.buttonText.text = "⌘";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardAltSprite);
				break;
			case KeyCode.Return:
				button.buttonText.text = "⏎";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardReturnSprite);
				break;
			case KeyCode.Backspace:
				button.buttonText.text = "⌫";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardShiftSprite);
				break;
			case KeyCode.UpArrow:
				button.buttonText.text = "↑";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.DownArrow:
				button.buttonText.text = "↓";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.LeftArrow:
				button.buttonText.text = "←";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.RightArrow:
				button.buttonText.text = "→";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.Keypad0:
				button.buttonText.text = "#0";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.Keypad1:
				button.buttonText.text = "#1";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.Keypad2:
				button.buttonText.text = "#2";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.Keypad3:
				button.buttonText.text = "#3";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.Keypad4:
				button.buttonText.text = "#4";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.Keypad5:
				button.buttonText.text = "#5";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.Keypad6:
				button.buttonText.text = "#6";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.Keypad7:
				button.buttonText.text = "#7";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.Keypad8:
				button.buttonText.text = "#8";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.Keypad9:
				button.buttonText.text = "#9";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.KeypadDivide:
				button.buttonText.text = "#/";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.KeypadMultiply:
				button.buttonText.text = "#*";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.KeypadEnter:
				button.buttonText.text = "#⏎";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.KeypadEquals:
				button.buttonText.text = "#=";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.KeypadMinus:
				button.buttonText.text = "#-";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.KeypadPlus:
				button.buttonText.text = "#+";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			case KeyCode.KeypadPeriod:
				button.buttonText.text = "#.";
				button.buttonText.enabled = true;
				button.SetImageSprite(KeyboardKeySprite);
				break;
			}
			break;
		}
		case ControllerType.Xbox:
			button.buttonText.enabled = false;
			switch (button.inputKey)
			{
			case InputEvent.InputKey.Jump:
			case InputEvent.InputKey.Accept:
				button.SetImageSprite(XboxButtonASprite);
				break;
			case InputEvent.InputKey.Suicide:
			case InputEvent.InputKey.Back:
				button.SetImageSprite(XboxButtonBSprite);
				break;
			case InputEvent.InputKey.Inventory:
				button.SetImageSprite(XboxButtonYSprite);
				break;
			case InputEvent.InputKey.Sprint:
				button.SetImageSprite(XboxButtonXSprite);
				break;
			case InputEvent.InputKey.RotateLeft:
				button.SetImageSprite(XboxButtonLBSprite);
				break;
			case InputEvent.InputKey.RotateRight:
				button.SetImageSprite(XboxButtonRBSprite);
				break;
			default:
				button.buttonText.enabled = true;
				button.buttonText.text = "???";
				button.SetImageSprite(KeyboardKeySprite);
				break;
			}
			break;
		case ControllerType.DualShock4:
			button.buttonText.enabled = false;
			switch (button.inputKey)
			{
			case InputEvent.InputKey.Accept:
				button.SetImageSprite(PS4ButtonXSprite);
				break;
			case InputEvent.InputKey.Jump:
				button.SetImageSprite(PS4ButtonXSprite);
				break;
			case InputEvent.InputKey.Back:
				button.SetImageSprite(PS4ButtonCircleSprite);
				break;
			case InputEvent.InputKey.Suicide:
				button.SetImageSprite(PS4ButtonCircleSprite);
				break;
			case InputEvent.InputKey.Inventory:
				button.SetImageSprite(PS4ButtonTriangleSprite);
				break;
			case InputEvent.InputKey.Sprint:
				button.SetImageSprite(PS4ButtonSquareSprite);
				break;
			case InputEvent.InputKey.RotateLeft:
				button.SetImageSprite(PS4ButtonL1Sprite);
				break;
			case InputEvent.InputKey.RotateRight:
				button.SetImageSprite(PS4ButtonR1Sprite);
				break;
			default:
				button.buttonText.enabled = true;
				button.buttonText.text = "???";
				button.SetImageSprite(KeyboardKeySprite);
				break;
			}
			break;
		case ControllerType.SingleJoyconL:
		case ControllerType.SingleJoyconR:
			button.buttonText.enabled = false;
			button.buttonText.text = "";
			switch (button.inputKey)
			{
			case InputEvent.InputKey.Jump:
			case InputEvent.InputKey.Accept:
				if (SwitchController.UseAltButtonLayout)
				{
					button.SetImageSprite(SwitchBottomButtonSprite);
				}
				else
				{
					button.SetImageSprite(SwitchRightButtonSprite);
				}
				break;
			case InputEvent.InputKey.Suicide:
			case InputEvent.InputKey.Back:
				if (SwitchController.UseAltButtonLayout)
				{
					button.SetImageSprite(SwitchRightButtonSprite);
				}
				else
				{
					button.SetImageSprite(SwitchBottomButtonSprite);
				}
				break;
			case InputEvent.InputKey.Inventory:
				if (SwitchController.UseAltButtonLayout)
				{
					button.SetImageSprite(SwitchTopButtonSprite);
				}
				else
				{
					button.SetImageSprite(SwitchLeftButtonSprite);
				}
				break;
			case InputEvent.InputKey.Sprint:
				if (SwitchController.UseAltButtonLayout)
				{
					button.SetImageSprite(SwitchLeftButtonSprite);
				}
				else
				{
					button.SetImageSprite(SwitchTopButtonSprite);
				}
				break;
			case InputEvent.InputKey.RotateLeft:
				button.SetImageSprite(SwitchButtonSLSprite);
				break;
			case InputEvent.InputKey.RotateRight:
				button.SetImageSprite(SwitchButtonSRSprite);
				break;
			default:
				button.buttonText.enabled = true;
				button.buttonText.text = "???";
				button.SetImageSprite(KeyboardKeySprite);
				break;
			}
			break;
		case ControllerType.Switch:
			button.buttonText.enabled = false;
			button.buttonText.text = "";
			switch (button.inputKey)
			{
			case InputEvent.InputKey.Jump:
			case InputEvent.InputKey.Accept:
				if (SwitchController.UseAltButtonLayout)
				{
					button.SetImageSprite(SwitchButtonBSprite);
				}
				else
				{
					button.SetImageSprite(SwitchButtonASprite);
				}
				break;
			case InputEvent.InputKey.Suicide:
			case InputEvent.InputKey.Back:
				if (SwitchController.UseAltButtonLayout)
				{
					button.SetImageSprite(SwitchButtonASprite);
				}
				else
				{
					button.SetImageSprite(SwitchButtonBSprite);
				}
				break;
			case InputEvent.InputKey.Inventory:
				if (SwitchController.UseAltButtonLayout)
				{
					button.SetImageSprite(SwitchButtonXSprite);
				}
				else
				{
					button.SetImageSprite(SwitchButtonYSprite);
				}
				break;
			case InputEvent.InputKey.Sprint:
				if (SwitchController.UseAltButtonLayout)
				{
					button.SetImageSprite(SwitchButtonYSprite);
				}
				else
				{
					button.SetImageSprite(SwitchButtonXSprite);
				}
				break;
			case InputEvent.InputKey.RotateLeft:
				button.SetImageSprite(SwitchButtonLSprite);
				break;
			case InputEvent.InputKey.RotateRight:
				button.SetImageSprite(SwitchButtonRSprite);
				break;
			default:
				button.buttonText.enabled = true;
				button.buttonText.text = "???";
				button.SetImageSprite(KeyboardKeySprite);
				break;
			}
			break;
		}
	}

	public static ControllerType GetControllerType(Controller controller)
	{
		if (GameState.DebugMode && GameSettings.GetInstance().forcedControllerVisual)
		{
			return GameSettings.GetInstance().forcedControllerType;
		}
		if (controller is KeyboardInput)
		{
			return ControllerType.KeyboardAndMouse;
		}
		InControlController inControlController = controller as InControlController;
		if (inControlController != null)
		{
			InputDeviceStyle deviceStyle = inControlController.GetInputDevice().DeviceStyle;
			if ((uint)(deviceStyle - 1) > 1u)
			{
				if ((uint)(deviceStyle - 4) <= 2u)
				{
					return ControllerType.DualShock4;
				}
				_ = 14;
			}
			return ControllerType.Xbox;
		}
		switch (controller.GetControllerType())
		{
		case Controller.ControllerType.SWITCH_DUAL:
		case Controller.ControllerType.SWITCH_FULL:
		case Controller.ControllerType.SWITCH_HANDHELD:
			return ControllerType.Switch;
		case Controller.ControllerType.SWITCH_LEFT:
			return ControllerType.SingleJoyconL;
		case Controller.ControllerType.SWITCH_RIGHT:
			return ControllerType.SingleJoyconR;
		default:
			return ControllerType.Xbox;
		}
	}
}
