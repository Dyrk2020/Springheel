using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;

public class KeyboardInput : InControlController
{
	private Dictionary<int, int> keys;

	private Dictionary<int, int> altKeys;

	public bool mouseActive;

	private Vector3 lastMousePos;

	private Vector3 lastMouseActivePos;

	public float MouseTimeout;

	private float mouseTimer;

	public float MouseMovementThreshold = 0.1f;

	public float MouseMaxDistance = 9f;

	public float MouseScreenScrollDistance = 16f;

	private InputEvent.InputKey[] keyEvents;

	private InputEvent[] eventPool = new InputEvent[10];

	public int framesSinceUnlock = 999;

	private int lastPoolInd;

	private HashSet<InputEvent.InputKey> deferredReleases = new HashSet<InputEvent.InputKey>();

	public static int[,] DefaultKeys = new int[22, 2]
	{
		{ 0, 119 },
		{ 1, 115 },
		{ 2, 97 },
		{ 3, 100 },
		{ 8, 32 },
		{ 9, 98 },
		{ 11, 304 },
		{ 12, 114 },
		{ 13, 122 },
		{ 16, 113 },
		{ 17, 101 },
		{ 18, 13 },
		{ 19, 27 },
		{ 20, 9 },
		{ 21, 13 },
		{ 22, 98 },
		{ 24, 105 },
		{ 25, 107 },
		{ 26, 106 },
		{ 27, 108 },
		{ 33, 116 },
		{ 14, 118 }
	};

	public static int[,] DefaultAltKeys = new int[22, 2]
	{
		{ 0, 273 },
		{ 1, 274 },
		{ 2, 276 },
		{ 3, 275 },
		{ 8, 0 },
		{ 9, 8 },
		{ 11, 0 },
		{ 12, 0 },
		{ 13, 0 },
		{ 16, 0 },
		{ 17, 0 },
		{ 18, 0 },
		{ 19, 0 },
		{ 20, 0 },
		{ 21, 32 },
		{ 22, 8 },
		{ 24, 264 },
		{ 25, 258 },
		{ 26, 260 },
		{ 27, 262 },
		{ 33, 0 },
		{ 14, 0 }
	};

	private InputEvent getFreeEvent(int player, InputEvent.InputKey key, float valuef, bool changed)
	{
		return getFreeEvent(player, key, valuef, valuef != 0f, changed);
	}

	private InputEvent getFreeEvent(int player, InputEvent.InputKey key, bool valueb, bool changed)
	{
		return getFreeEvent(player, key, valueb ? 1 : 0, valueb, changed);
	}

	private InputEvent getFreeEvent(int player, InputEvent.InputKey key, float valuef, bool valueb, bool changed)
	{
		for (int i = lastPoolInd; i != eventPool.Length; i++)
		{
			InputEvent inputEvent = eventPool[i];
			if (inputEvent.Consumed)
			{
				inputEvent.Reset(player, key, valuef, valueb, changed);
				lastPoolInd = i;
				return inputEvent;
			}
		}
		for (int j = 0; j != lastPoolInd; j++)
		{
			InputEvent inputEvent2 = eventPool[j];
			if (inputEvent2.Consumed)
			{
				inputEvent2.Reset(player, key, valuef, valueb, changed);
				lastPoolInd = j;
				return inputEvent2;
			}
		}
		return new InputEvent(player, key, valuef, valueb, changed);
	}

	public override ControllerType GetControllerType()
	{
		return ControllerType.KEYBOARD;
	}

	public override void AddReceiver(InputReceiver r)
	{
		base.AddReceiver(r);
		NotifyNextFrame(getFreeEvent(Player, InputEvent.InputKey.ChangeMode, IsUsingPosition(), changed: true));
	}

	public override InputDevice GetInputDevice()
	{
		return null;
	}

	public override void SetInputDevice(InputDevice d)
	{
	}

	public override void Awake()
	{
		base.Awake();
		keyEvents = (InputEvent.InputKey[])Enum.GetValues(typeof(InputEvent.InputKey));
		for (int i = 0; i != eventPool.Length; i++)
		{
			eventPool[i] = new InputEvent();
		}
		keys = new Dictionary<int, int>();
		altKeys = new Dictionary<int, int>();
		keys.Add(0, 119);
		keys.Add(1, 115);
		keys.Add(2, 97);
		keys.Add(3, 100);
		keys.Add(4, 119);
		keys.Add(5, 115);
		keys.Add(6, 97);
		keys.Add(7, 100);
		keys.Add(28, 49);
		keys.Add(29, 51);
		keys.Add(30, 52);
		keys.Add(31, 50);
		keys.Add(8, 32);
		keys.Add(9, 98);
		keys.Add(11, 304);
		keys.Add(12, 114);
		keys.Add(13, 122);
		keys.Add(16, 113);
		keys.Add(17, 101);
		keys.Add(18, 13);
		keys.Add(19, 27);
		keys.Add(20, 9);
		keys.Add(21, 32);
		keys.Add(34, 27);
		keys.Add(22, 98);
		keys.Add(24, 105);
		keys.Add(25, 107);
		keys.Add(26, 106);
		keys.Add(27, 108);
		keys.Add(14, 118);
		keys.Add(33, 116);
		altKeys.Add(0, 273);
		altKeys.Add(1, 274);
		altKeys.Add(2, 276);
		altKeys.Add(3, 275);
		altKeys.Add(4, 273);
		altKeys.Add(5, 274);
		altKeys.Add(6, 276);
		altKeys.Add(7, 275);
		altKeys.Add(8, 0);
		altKeys.Add(9, 8);
		altKeys.Add(11, 0);
		altKeys.Add(12, 0);
		altKeys.Add(13, 0);
		altKeys.Add(16, 0);
		altKeys.Add(17, 0);
		altKeys.Add(18, 0);
		altKeys.Add(19, 0);
		altKeys.Add(20, 0);
		altKeys.Add(21, 0);
		altKeys.Add(22, 8);
		altKeys.Add(24, 264);
		altKeys.Add(25, 258);
		altKeys.Add(26, 260);
		altKeys.Add(27, 262);
		altKeys.Add(33, 0);
		altKeys.Add(14, 0);
		lastMousePos = Input.mousePosition;
	}

	private bool RelatedInputKeyPressed(int inputKey)
	{
		if (!StandardKeyPressed(inputKey))
		{
			return AltKeyPressed(inputKey);
		}
		return true;
	}

	private bool StandardKeyPressed(int inputKey)
	{
		if (!keys.ContainsKey(inputKey))
		{
			return false;
		}
		return Input.GetKey((KeyCode)keys[inputKey]);
	}

	private bool AltKeyPressed(int inputKey)
	{
		if (!altKeys.ContainsKey(inputKey))
		{
			return false;
		}
		return Input.GetKey((KeyCode)altKeys[inputKey]);
	}

	private bool RelatedInputKeyDown(int inputKey)
	{
		if (NoKeyJustPressed(inputKey))
		{
			return false;
		}
		if (BothKeysJustPressed(inputKey))
		{
			return true;
		}
		if (StandardKeyJustPressed(inputKey))
		{
			return !AltKeyPressed(inputKey);
		}
		if (AltKeyJustPressed(inputKey))
		{
			return !StandardKeyPressed(inputKey);
		}
		return false;
	}

	private bool NoKeyJustPressed(int inputKey)
	{
		if (!StandardKeyJustPressed(inputKey))
		{
			return !AltKeyJustPressed(inputKey);
		}
		return false;
	}

	private bool BothKeysJustPressed(int inputKey)
	{
		if (StandardKeyJustPressed(inputKey))
		{
			return AltKeyJustPressed(inputKey);
		}
		return false;
	}

	private bool NoKeyJustReleased(int inputKey)
	{
		if (!StandardKeyJustReleased(inputKey))
		{
			return !AltKeyJustReleased(inputKey);
		}
		return false;
	}

	private bool NoKeyIsCurrentlyHeld(int inputKey)
	{
		if (!StandardKeyPressed(inputKey))
		{
			return !AltKeyPressed(inputKey);
		}
		return false;
	}

	private bool StandardKeyJustPressed(int inputKey)
	{
		if (keys.ContainsKey(inputKey))
		{
			return Input.GetKeyDown((KeyCode)keys[inputKey]);
		}
		return false;
	}

	private bool AltKeyJustPressed(int inputKey)
	{
		if (altKeys.ContainsKey(inputKey))
		{
			return Input.GetKeyDown((KeyCode)altKeys[inputKey]);
		}
		return false;
	}

	private bool RelatedInputKeyUp(int inputKey)
	{
		bool flag = StandardKeyJustReleased(inputKey);
		bool flag2 = AltKeyJustReleased(inputKey);
		if (!flag && !flag2)
		{
			return false;
		}
		if (flag && flag2)
		{
			return true;
		}
		if (flag)
		{
			return !AltKeyPressed(inputKey);
		}
		if (flag2)
		{
			return !StandardKeyPressed(inputKey);
		}
		return false;
	}

	private bool StandardKeyJustReleased(int inputKey)
	{
		if (keys.ContainsKey(inputKey))
		{
			return Input.GetKeyUp((KeyCode)keys[inputKey]);
		}
		return false;
	}

	private bool AltKeyJustReleased(int inputKey)
	{
		if (altKeys.ContainsKey(inputKey))
		{
			return Input.GetKeyUp((KeyCode)altKeys[inputKey]);
		}
		return false;
	}

	private bool InputSupported(InputEvent.InputKey inputKey)
	{
		if (!keys.ContainsKey((int)inputKey))
		{
			return altKeys.ContainsKey((int)inputKey);
		}
		return true;
	}

	public override void Update()
	{
		base.Update();
		if (Controller.InputFieldIsActive)
		{
			framesSinceUnlock = 0;
		}
		else
		{
			framesSinceUnlock++;
		}
		bool flag = false;
		if (!Controller.InputFieldIsActive)
		{
			if (Controller.justUnlocked)
			{
				Controller.justUnlocked = false;
			}
			else
			{
				InputEvent.InputKey[] array = keyEvents;
				foreach (InputEvent.InputKey inputKey in array)
				{
					if (inputKey == InputEvent.InputKey.NoKey || !InputSupported(inputKey))
					{
						continue;
					}
					if (RelatedInputKeyPressed((int)inputKey))
					{
						Notify(getFreeEvent(Player, inputKey, valueb: true, RelatedInputKeyDown((int)inputKey)));
						if (inputKey == InputEvent.InputKey.Up || inputKey == InputEvent.InputKey.Down || inputKey == InputEvent.InputKey.Left || inputKey == InputEvent.InputKey.Right || inputKey == InputEvent.InputKey.OrthoUp || inputKey == InputEvent.InputKey.OrthoDown || inputKey == InputEvent.InputKey.OrthoLeft || inputKey == InputEvent.InputKey.OrthoRight)
						{
							flag = true;
						}
					}
					if (RelatedInputKeyUp((int)inputKey))
					{
						if (RelatedInputKeyDown((int)inputKey))
						{
							deferredReleases.Add(inputKey);
						}
						else
						{
							Notify(getFreeEvent(Player, inputKey, valueb: false, changed: true));
						}
					}
					else if (deferredReleases.Contains(inputKey))
					{
						Notify(getFreeEvent(Player, inputKey, valueb: false, changed: true));
						deferredReleases.Remove(inputKey);
					}
				}
			}
		}
		float sqrMagnitude = (Input.mousePosition - lastMousePos).sqrMagnitude;
		float sqrMagnitude2 = (Input.mousePosition - lastMouseActivePos).sqrMagnitude;
		bool flag2 = sqrMagnitude > 0f;
		if (!mouseActive && flag2)
		{
			bool num = sqrMagnitude > MouseMovementThreshold * MouseMovementThreshold;
			bool flag3 = sqrMagnitude2 > MouseMaxDistance * MouseMaxDistance;
			if (num || flag3 || usePreciseCursor)
			{
				mouseActive = true;
				Notify(getFreeEvent(Player, InputEvent.InputKey.ChangeMode, valueb: true, changed: true));
			}
		}
		if (Input.GetMouseButton(0))
		{
			if (!mouseActive)
			{
				mouseActive = true;
				Notify(getFreeEvent(Player, InputEvent.InputKey.ChangeMode, valueb: true, changed: true));
			}
			Notify(getFreeEvent(Player, InputEvent.InputKey.Accept, valueb: true, Input.GetMouseButtonDown(0)));
		}
		else if (Input.GetMouseButtonUp(0))
		{
			Notify(getFreeEvent(Player, InputEvent.InputKey.Accept, valueb: false, changed: true));
		}
		if (!Controller.InputFieldIsActive)
		{
			if (Input.GetMouseButton(1))
			{
				Notify(getFreeEvent(Player, InputEvent.InputKey.Back, valueb: true, Input.GetMouseButtonDown(1)));
				Notify(getFreeEvent(Player, InputEvent.InputKey.Suicide, valueb: true, Input.GetMouseButtonDown(1)));
			}
			else if (Input.GetMouseButtonUp(1))
			{
				Notify(getFreeEvent(Player, InputEvent.InputKey.Back, valueb: false, changed: true));
				Notify(getFreeEvent(Player, InputEvent.InputKey.Suicide, valueb: false, changed: true));
			}
			if (Input.mouseScrollDelta.y >= 1f)
			{
				Notify(getFreeEvent(Player, InputEvent.InputKey.RotateRight, valueb: true, changed: true));
				Notify(getFreeEvent(Player, InputEvent.InputKey.RotateRight, valueb: false, changed: true));
			}
			if (Input.mouseScrollDelta.y <= -1f)
			{
				Notify(getFreeEvent(Player, InputEvent.InputKey.RotateLeft, valueb: true, changed: true));
				Notify(getFreeEvent(Player, InputEvent.InputKey.RotateLeft, valueb: false, changed: true));
			}
		}
		if (mouseActive && !flag2)
		{
			mouseTimer += Time.unscaledDeltaTime;
			if ((!(Input.mousePosition.x <= MouseScreenScrollDistance) && !(Input.mousePosition.x >= (float)Screen.width - MouseScreenScrollDistance) && !(Input.mousePosition.y <= MouseScreenScrollDistance) && !(Input.mousePosition.y >= (float)Screen.height - MouseScreenScrollDistance) && mouseTimer >= MouseTimeout) || flag)
			{
				mouseTimer = 0f;
				mouseActive = false;
				Notify(getFreeEvent(Player, InputEvent.InputKey.ChangeMode, valueb: false, changed: true));
			}
		}
		lastMousePos = Input.mousePosition;
		if (mouseActive)
		{
			lastMouseActivePos = Input.mousePosition;
		}
	}

	public void RebindKey(InputEvent.InputKey eventKey, KeyCode keycode, bool force = false)
	{
		if (eventKey != InputEvent.InputKey.NoKey && (force || eventKey != InputEvent.InputKey.Accept))
		{
			keys[(int)eventKey] = (int)keycode;
			if (eventKey == InputEvent.InputKey.Jump)
			{
				RebindKey(InputEvent.InputKey.Accept, keycode, force: true);
			}
		}
	}

	public void RebindAltKey(InputEvent.InputKey eventKey, KeyCode keycode, bool force = false)
	{
		if (eventKey != InputEvent.InputKey.NoKey && (force || eventKey != InputEvent.InputKey.Accept))
		{
			if (altKeys.ContainsKey((int)eventKey))
			{
				altKeys[(int)eventKey] = (int)keycode;
			}
			else
			{
				altKeys.Add((int)eventKey, (int)keycode);
			}
			if (eventKey == InputEvent.InputKey.Jump)
			{
				RebindAltKey(InputEvent.InputKey.Accept, keycode, force: true);
			}
		}
	}

	public KeyCode? GetKeyBinding(InputEvent.InputKey eventKey)
	{
		if (eventKey == InputEvent.InputKey.NoKey)
		{
			return null;
		}
		if (keys.ContainsKey((int)eventKey))
		{
			return (KeyCode)keys[(int)eventKey];
		}
		return null;
	}

	public KeyCode? GetAltKeyBinding(InputEvent.InputKey eventKey)
	{
		if (eventKey == InputEvent.InputKey.NoKey || !altKeys.ContainsKey((int)eventKey))
		{
			return null;
		}
		return (KeyCode)altKeys[(int)eventKey];
	}

	public override bool IsUsingPosition()
	{
		return mouseActive;
	}

	public override Vector2 GetVector(bool absolute = false)
	{
		if (mouseActive)
		{
			if (absolute)
			{
				return Input.mousePosition;
			}
			return new Vector2(Input.GetAxis("mouse x") * 2f, Input.GetAxis("mouse y") * 2f);
		}
		return new Vector2((Input.GetKey(KeyCode.A) ? (-1) : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0), (Input.GetKey(KeyCode.S) ? (-1) : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0));
	}

	public override void Reset()
	{
		associatedChars = new Character.Animals[4];
		Player = 0;
		assumeUser = false;
	}
}
