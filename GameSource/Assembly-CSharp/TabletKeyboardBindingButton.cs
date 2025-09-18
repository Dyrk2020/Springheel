using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class TabletKeyboardBindingButton : TabletButton, InputReceiver
{
	public InputEvent.InputKey inputKey;

	public KeyCode boundKey;

	public bool isAltKey;

	public TabletTextLabel textLabel;

	private KeyboardInput keyboard;

	private bool initialized;

	private bool rebinding;

	private bool cancel;

	private void Start()
	{
		if (!initialized)
		{
			Initialize();
		}
	}

	public void Initialize()
	{
		RefreshBinding();
		keyboard = GameState.GetInstance().Keyboard;
		keyboard.AddReceiver(this);
		initialized = true;
	}

	public void RefreshBinding()
	{
		KeyboardInput keyboardInput = GameState.GetInstance().Keyboard;
		boundKey = ((!isAltKey) ? keyboardInput.GetKeyBinding(inputKey).GetValueOrDefault() : keyboardInput.GetAltKeyBinding(inputKey).GetValueOrDefault());
		UpdateButtonText();
	}

	private void UpdateButtonText()
	{
		switch (boundKey)
		{
		case KeyCode.None:
			textLabel.text = "...";
			break;
		case KeyCode.UpArrow:
			textLabel.text = "↑";
			break;
		case KeyCode.DownArrow:
			textLabel.text = "↓";
			break;
		case KeyCode.LeftArrow:
			textLabel.text = "←";
			break;
		case KeyCode.RightArrow:
			textLabel.text = "→";
			break;
		case KeyCode.Keypad0:
		case KeyCode.Keypad1:
		case KeyCode.Keypad2:
		case KeyCode.Keypad3:
		case KeyCode.Keypad4:
		case KeyCode.Keypad5:
		case KeyCode.Keypad6:
		case KeyCode.Keypad7:
		case KeyCode.Keypad8:
		case KeyCode.Keypad9:
			textLabel.text = boundKey.ToString().Replace("Keypad", "No.");
			break;
		case KeyCode.Alpha0:
		case KeyCode.Alpha1:
		case KeyCode.Alpha2:
		case KeyCode.Alpha3:
		case KeyCode.Alpha4:
		case KeyCode.Alpha5:
		case KeyCode.Alpha6:
		case KeyCode.Alpha7:
		case KeyCode.Alpha8:
		case KeyCode.Alpha9:
			textLabel.text = boundKey.ToString().Replace("Alpha", "");
			break;
		default:
			textLabel.text = boundKey.ToString();
			break;
		}
		textLabel.UpdateDynamicText();
	}

	private IEnumerator waitForNoKeys()
	{
		bool keyPressed = Input.anyKey;
		while (keyPressed)
		{
			keyPressed = false;
			foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
			{
				if (!IsIgnoredKey(value) && Input.GetKeyDown(value))
				{
					keyPressed = true;
					break;
				}
			}
			yield return null;
		}
		StartCoroutine(waitForKeyPress());
	}

	private IEnumerator waitForKeyPress()
	{
		bool keyPressed = false;
		while (!keyPressed && !cancel)
		{
			if (Input.anyKeyDown)
			{
				foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
				{
					if (IsIgnoredKey(value) || !Input.GetKeyDown(value))
					{
						continue;
					}
					KeyCode keycode = value;
					keyPressed = true;
					if (isAltKey)
					{
						Debug.Log(boundKey.ToString() + " rebound from " + keyboard.GetAltKeyBinding(inputKey).ToString() + " to " + keycode);
					}
					else
					{
						Debug.Log(boundKey.ToString() + " rebound from " + keyboard.GetKeyBinding(inputKey).ToString() + " to " + keycode);
					}
					if (isAltKey)
					{
						keyboard.RebindAltKey(inputKey, keycode);
						if (inputKey == InputEvent.InputKey.Up)
						{
							keyboard.RebindAltKey(InputEvent.InputKey.OrthoUp, keycode);
						}
						if (inputKey == InputEvent.InputKey.Down)
						{
							keyboard.RebindAltKey(InputEvent.InputKey.OrthoDown, keycode);
						}
						if (inputKey == InputEvent.InputKey.Left)
						{
							keyboard.RebindAltKey(InputEvent.InputKey.OrthoLeft, keycode);
						}
						if (inputKey == InputEvent.InputKey.Right)
						{
							keyboard.RebindAltKey(InputEvent.InputKey.OrthoRight, keycode);
						}
						if (inputKey == InputEvent.InputKey.Suicide)
						{
							keyboard.RebindAltKey(InputEvent.InputKey.Back, keycode);
						}
					}
					else
					{
						keyboard.RebindKey(inputKey, keycode);
						if (inputKey == InputEvent.InputKey.Up)
						{
							keyboard.RebindKey(InputEvent.InputKey.OrthoUp, keycode);
						}
						if (inputKey == InputEvent.InputKey.Down)
						{
							keyboard.RebindKey(InputEvent.InputKey.OrthoDown, keycode);
						}
						if (inputKey == InputEvent.InputKey.Left)
						{
							keyboard.RebindKey(InputEvent.InputKey.OrthoLeft, keycode);
						}
						if (inputKey == InputEvent.InputKey.Right)
						{
							keyboard.RebindKey(InputEvent.InputKey.OrthoRight, keycode);
						}
						if (inputKey == InputEvent.InputKey.Suicide)
						{
							keyboard.RebindKey(InputEvent.InputKey.Back, keycode);
						}
					}
					boundKey = keycode;
					break;
				}
			}
			yield return null;
		}
		UpdateSaveData();
		UpdateButtonText();
		AkSoundEngine.PostEvent("UI_UPad_Options_Control_Enter", base.gameObject);
		cancel = false;
		rebinding = false;
		StartCoroutine(unfreezeNextFrame());
	}

	private IEnumerator unfreezeNextFrame()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForEndOfFrame();
		GetComponentInParent<InventoryBook>().FrozenOnPage = false;
	}

	public void OnClickButton(PickCursor pickCursor)
	{
		GetComponentInParent<TabletKeyboardConfig>().CancelCurrentBinding();
		textLabel.text = "...?";
		rebinding = true;
		GetComponentInParent<InventoryBook>().FrozenOnPage = true;
		Debug.Log((isAltKey ? "Alt " : "") + inputKey.ToString() + " Waiting for Rebind");
		StartCoroutine(waitForNoKeys());
	}

	public void CancelRebind()
	{
		if (rebinding)
		{
			cancel = true;
			StartCoroutine(unfreezeNextFrame());
		}
	}

	public override void Update()
	{
		base.Update();
		if (rebinding || !initialized)
		{
			return;
		}
		if (isAltKey)
		{
			boundKey = keyboard.GetAltKeyBinding(inputKey).GetValueOrDefault();
		}
		else
		{
			boundKey = keyboard.GetKeyBinding(inputKey).GetValueOrDefault();
		}
		if (base.HasTrackedCursors && trackedCursors.FirstOrDefault((PickCursor c) => c.LocalPlayer.UseController is KeyboardInput) != null && Input.GetKeyDown(KeyCode.Delete))
		{
			boundKey = KeyCode.None;
			if (isAltKey)
			{
				keyboard.RebindAltKey(inputKey, KeyCode.None);
			}
			else
			{
				keyboard.RebindKey(inputKey, KeyCode.None);
			}
			UpdateSaveData();
		}
		UpdateButtonText();
	}

	private void UpdateSaveData()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if (isAltKey)
		{
			for (int i = 0; i != saveFileDataForMainUser.DefaultAltKeys.GetLength(0); i++)
			{
				if (saveFileDataForMainUser.DefaultAltKeys[i, 0] == (int)inputKey)
				{
					saveFileDataForMainUser.DefaultAltKeys[i, 1] = (int)boundKey;
				}
			}
			return;
		}
		for (int j = 0; j != saveFileDataForMainUser.DefaultKeys.GetLength(0); j++)
		{
			if (saveFileDataForMainUser.DefaultKeys[j, 0] == (int)inputKey)
			{
				saveFileDataForMainUser.DefaultKeys[j, 1] = (int)boundKey;
			}
		}
	}

	private bool IsIgnoredKey(KeyCode keyCode)
	{
		if (keyCode >= KeyCode.Mouse0 && keyCode <= KeyCode.Mouse6)
		{
			return true;
		}
		if (keyCode >= KeyCode.JoystickButton0 && keyCode <= KeyCode.Joystick8Button19)
		{
			return true;
		}
		return false;
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (e.Key == InputEvent.InputKey.Esc && rebinding)
		{
			cancel = true;
			StartCoroutine(unfreezeNextFrame());
		}
	}
}
