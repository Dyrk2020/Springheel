using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PickableKeyboardBinding : PickableButton, InputReceiver
{
	public KeyCode? KeyboardButton;

	public InputEvent.InputKey KeyEvent;

	public bool IsAltKey;

	private KeyboardInput keyboard;

	private bool rebinding;

	private bool cancel;

	private KeyCode[] ignoreKeys = new KeyCode[7]
	{
		KeyCode.Mouse0,
		KeyCode.Mouse1,
		KeyCode.Mouse2,
		KeyCode.Mouse3,
		KeyCode.Mouse4,
		KeyCode.Mouse5,
		KeyCode.Mouse6
	};

	protected override void Awake()
	{
		if (buttonText == null)
		{
			buttonText = GetComponent<Text>();
		}
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		if (IsAltKey)
		{
			KeyboardButton = GameState.GetInstance().Keyboard.GetAltKeyBinding(KeyEvent);
		}
		else
		{
			KeyboardButton = GameState.GetInstance().Keyboard.GetKeyBinding(KeyEvent);
		}
		keyboard = GameState.GetInstance().Keyboard;
		keyboard.AddReceiver(this);
		setText();
	}

	protected override void Update()
	{
		base.Update();
		if (rebinding || !Visible || !initialized)
		{
			return;
		}
		if (IsAltKey)
		{
			KeyboardButton = GameState.GetInstance().Keyboard.GetAltKeyBinding(KeyEvent);
		}
		else
		{
			KeyboardButton = GameState.GetInstance().Keyboard.GetKeyBinding(KeyEvent);
		}
		if (HoveredCursors.Count > 0 && HoveredCursors.Find((Cursor c) => c.LocalPlayer.UseController is KeyboardInput) != null && Input.GetKeyDown(KeyCode.Delete))
		{
			KeyboardButton = KeyCode.None;
			if (IsAltKey)
			{
				keyboard.RebindAltKey(KeyEvent, KeyCode.None);
			}
			else
			{
				keyboard.RebindKey(KeyEvent, KeyCode.None);
			}
			UpdateSaveData();
		}
		setText();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		buttonText.text = "...?";
		rebinding = true;
		inventoryBook.FrozenOnPage = true;
		PickableButton.maskAll = true;
		Debug.Log((IsAltKey ? "Alt " : "") + KeyEvent.ToString() + " Waiting for Rebind");
		StartCoroutine(waitForNoKeys());
	}

	private void setText()
	{
		if (!(buttonText == null))
		{
			if (KeyboardButton == KeyCode.None)
			{
				buttonText.text = "...";
			}
			else if (KeyboardButton == KeyCode.UpArrow)
			{
				buttonText.text = "↑";
			}
			else if (KeyboardButton == KeyCode.DownArrow)
			{
				buttonText.text = "↓";
			}
			else if (KeyboardButton == KeyCode.LeftArrow)
			{
				buttonText.text = "←";
			}
			else if (KeyboardButton == KeyCode.RightArrow)
			{
				buttonText.text = "→";
			}
			else
			{
				buttonText.text = KeyboardButton.ToString();
			}
			buttonText.text = buttonText.text.Replace("Keypad", "No.");
			buttonText.text = buttonText.text.Replace("Alpha", "");
		}
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
					bool flag = false;
					for (int i = 0; i != ignoreKeys.Length; i++)
					{
						if (ignoreKeys[i] == value)
						{
							flag = true;
							break;
						}
					}
					if (value.ToString().Contains("Joystick"))
					{
						flag = true;
					}
					if (flag || !Input.GetKeyDown(value))
					{
						continue;
					}
					KeyCode keyCode2 = value;
					keyPressed = true;
					if (IsAltKey)
					{
						Debug.Log(KeyEvent.ToString() + " rebound from " + keyboard.GetAltKeyBinding(KeyEvent).ToString() + " to " + keyCode2);
					}
					else
					{
						Debug.Log(KeyEvent.ToString() + " rebound from " + keyboard.GetKeyBinding(KeyEvent).ToString() + " to " + keyCode2);
					}
					if (IsAltKey)
					{
						keyboard.RebindAltKey(KeyEvent, keyCode2);
						if (KeyEvent == InputEvent.InputKey.Up)
						{
							keyboard.RebindAltKey(InputEvent.InputKey.OrthoUp, keyCode2);
						}
						if (KeyEvent == InputEvent.InputKey.Down)
						{
							keyboard.RebindAltKey(InputEvent.InputKey.OrthoDown, keyCode2);
						}
						if (KeyEvent == InputEvent.InputKey.Left)
						{
							keyboard.RebindAltKey(InputEvent.InputKey.OrthoLeft, keyCode2);
						}
						if (KeyEvent == InputEvent.InputKey.Right)
						{
							keyboard.RebindAltKey(InputEvent.InputKey.OrthoRight, keyCode2);
						}
						if (KeyEvent == InputEvent.InputKey.Suicide)
						{
							keyboard.RebindAltKey(InputEvent.InputKey.Back, keyCode2);
						}
					}
					else
					{
						keyboard.RebindKey(KeyEvent, keyCode2);
						if (KeyEvent == InputEvent.InputKey.Up)
						{
							keyboard.RebindKey(InputEvent.InputKey.OrthoUp, keyCode2);
						}
						if (KeyEvent == InputEvent.InputKey.Down)
						{
							keyboard.RebindKey(InputEvent.InputKey.OrthoDown, keyCode2);
						}
						if (KeyEvent == InputEvent.InputKey.Left)
						{
							keyboard.RebindKey(InputEvent.InputKey.OrthoLeft, keyCode2);
						}
						if (KeyEvent == InputEvent.InputKey.Right)
						{
							keyboard.RebindKey(InputEvent.InputKey.OrthoRight, keyCode2);
						}
						if (KeyEvent == InputEvent.InputKey.Suicide)
						{
							keyboard.RebindKey(InputEvent.InputKey.Back, keyCode2);
						}
					}
					KeyboardButton = keyCode2;
					break;
				}
			}
			yield return null;
		}
		UpdateSaveData();
		setText();
		cancel = false;
		rebinding = false;
		StartCoroutine(unfreezeNextFrame());
	}

	private void UpdateSaveData()
	{
		SaveFileData saveFileDataForMainUser = StatTracker.Instance.GetSaveFileDataForMainUser();
		if (IsAltKey)
		{
			for (int i = 0; i != saveFileDataForMainUser.DefaultAltKeys.GetLength(0); i++)
			{
				if (saveFileDataForMainUser.DefaultAltKeys[i, 0] == (int)KeyEvent)
				{
					saveFileDataForMainUser.DefaultAltKeys[i, 1] = (int)KeyboardButton.Value;
				}
			}
			return;
		}
		for (int j = 0; j != saveFileDataForMainUser.DefaultKeys.GetLength(0); j++)
		{
			if (saveFileDataForMainUser.DefaultKeys[j, 0] == (int)KeyEvent)
			{
				saveFileDataForMainUser.DefaultKeys[j, 1] = (int)KeyboardButton.Value;
			}
		}
	}

	private IEnumerator unfreezeNextFrame()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForEndOfFrame();
		inventoryBook.FrozenOnPage = false;
		PickableButton.ResetMasks();
	}

	private IEnumerator waitForNoKeys()
	{
		bool keyPressed = Input.anyKey;
		while (keyPressed)
		{
			keyPressed = false;
			foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
			{
				bool flag = false;
				for (int i = 0; i != ignoreKeys.Length; i++)
				{
					if (ignoreKeys[i] == value)
					{
						flag = true;
						break;
					}
				}
				if (value.ToString().Contains("Joystick"))
				{
					flag = true;
				}
				if (!flag && Input.GetKeyDown(value))
				{
					keyPressed = true;
				}
			}
			yield return null;
		}
		StartCoroutine(waitForKeyPress());
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
