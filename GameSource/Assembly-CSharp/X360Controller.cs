using System;
using InControl;
using UnityEngine;

public class X360Controller : InControlController
{
	private float triggerDeadZone = 0.25f;

	private float analogueDeadZone = 0.2f;

	private float orthoDirectionSelectionZone = 0.9f;

	private bool lastFrameUp;

	private bool lastFrameDown;

	private bool lastFrameLeft;

	private bool lastFrameRight;

	private bool lastFrameOrthoUp;

	private bool lastFrameOrthoDown;

	private bool lastFrameOrthoLeft;

	private bool lastFrameOrthoRight;

	private bool lastFrameOrthoUp2;

	private bool lastFrameOrthoDown2;

	private bool lastFrameOrthoLeft2;

	private bool lastFrameOrthoRight2;

	private bool lastFrameLT;

	private bool lastFrameRT;

	private bool lastFrameUp2;

	private bool lastFrameDown2;

	private bool lastFrameLeft2;

	private bool lastFrameRight2;

	private InputDevice device;

	public bool Attached;

	public string Device;

	private const float PiOverFour = MathF.PI / 4f;

	private InputEvent.InputKey Action1MenuInputKey => InputEvent.InputKey.Accept;

	private InputEvent.InputKey Action2MenuInputKey => InputEvent.InputKey.Back;

	public override ControllerType GetControllerType()
	{
		if (device.Name.Contains("XBOX One"))
		{
			return ControllerType.XBOXONE;
		}
		return ControllerType.XBOX360;
	}

	public override void SetInputDevice(InputDevice d)
	{
		if (d != device)
		{
			Debug.Log("Device for controller " + base.name + " changed from " + device?.ToString() + " to " + d);
		}
		device = d;
	}

	public override InputDevice GetInputDevice()
	{
		return device;
	}

	public override void Update()
	{
		base.Update();
		if (device == null)
		{
			lastFrameUp = false;
			lastFrameDown = false;
			lastFrameLeft = false;
			lastFrameRight = false;
			lastFrameOrthoUp = false;
			lastFrameOrthoDown = false;
			lastFrameOrthoLeft = false;
			lastFrameOrthoRight = false;
			lastFrameOrthoUp2 = false;
			lastFrameOrthoDown2 = false;
			lastFrameOrthoLeft2 = false;
			lastFrameOrthoRight2 = false;
			lastFrameLT = false;
			lastFrameRT = false;
			lastFrameUp2 = false;
			lastFrameDown2 = false;
			lastFrameLeft2 = false;
			lastFrameRight2 = false;
			return;
		}
		Attached = device.IsAttached;
		Device = device.Name;
		Vector2 point = device.Direction.Vector;
		float num;
		float y;
		if (!GameSettings.GetInstance().newDiagonalMapping)
		{
			if (point.sqrMagnitude < analogueDeadZone * analogueDeadZone)
			{
				point = Vector2.zero;
			}
			num = point.x;
			y = point.y;
			if (y <= -0.7f)
			{
				num /= 0.7f;
			}
		}
		else
		{
			point = ((!(point.magnitude < analogueDeadZone)) ? CircleToSquare(point, 4.0) : Vector2.zero);
			num = point.x;
			y = point.y;
		}
		if (num < 0f)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Left, 0f - num, !lastFrameLeft));
			Notify(new InputEvent(Player, InputEvent.InputKey.Right, 0f, lastFrameRight));
			lastFrameLeft = true;
			lastFrameRight = false;
		}
		else if (num > 0f)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Right, num, !lastFrameRight));
			Notify(new InputEvent(Player, InputEvent.InputKey.Left, 0f, lastFrameLeft));
			lastFrameRight = true;
			lastFrameLeft = false;
		}
		else
		{
			if (lastFrameLeft)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.Left, 0f, changed: true));
			}
			if (lastFrameRight)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.Right, 0f, changed: true));
			}
			lastFrameLeft = false;
			lastFrameRight = false;
		}
		if (y > 0f)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Up, y, !lastFrameUp));
			Notify(new InputEvent(Player, InputEvent.InputKey.Down, 0f, lastFrameDown));
			lastFrameUp = true;
			lastFrameDown = false;
		}
		else if (y < 0f)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Down, 0f - y, !lastFrameDown));
			Notify(new InputEvent(Player, InputEvent.InputKey.Up, 0f, lastFrameUp));
			lastFrameDown = true;
			lastFrameUp = false;
		}
		else
		{
			if (lastFrameUp)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.Up, 0f, changed: true));
			}
			if (lastFrameDown)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.Down, 0f, changed: true));
			}
			lastFrameUp = false;
			lastFrameDown = false;
		}
		if (num != 0f || y != 0f)
		{
			if (Mathf.Abs(num) > orthoDirectionSelectionZone)
			{
				if (num > 0f)
				{
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoRight, 1f, !lastFrameOrthoRight));
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoLeft, 0f, lastFrameOrthoLeft));
					lastFrameOrthoRight = true;
					lastFrameOrthoLeft = false;
				}
				else
				{
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoLeft, 1f, !lastFrameOrthoLeft));
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoRight, 0f, lastFrameOrthoRight));
					lastFrameOrthoLeft = true;
					lastFrameOrthoRight = false;
				}
			}
			else if (Mathf.Abs(y) > orthoDirectionSelectionZone)
			{
				if (y > 0f)
				{
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoUp, 1f, !lastFrameOrthoUp));
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoDown, 0f, lastFrameOrthoDown));
					lastFrameOrthoUp = true;
					lastFrameOrthoDown = false;
				}
				else
				{
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoDown, 1f, !lastFrameOrthoDown));
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoUp, 0f, lastFrameOrthoUp));
					lastFrameOrthoDown = true;
					lastFrameOrthoUp = false;
				}
			}
		}
		else
		{
			if (lastFrameOrthoUp)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.OrthoUp, 0f, changed: true));
			}
			if (lastFrameOrthoDown)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.OrthoDown, 0f, changed: true));
			}
			if (lastFrameOrthoLeft)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.OrthoLeft, 0f, changed: true));
			}
			if (lastFrameOrthoRight)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.OrthoRight, 0f, changed: true));
			}
			lastFrameOrthoUp = false;
			lastFrameOrthoDown = false;
			lastFrameOrthoLeft = false;
			lastFrameOrthoRight = false;
		}
		Vector2 vector = device.RightStick.Vector;
		if (vector.sqrMagnitude < analogueDeadZone * analogueDeadZone)
		{
			vector = Vector2.zero;
		}
		float x = vector.x;
		float y2 = vector.y;
		if (x < 0f)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Left2, 0f - x, !lastFrameLeft2));
			Notify(new InputEvent(Player, InputEvent.InputKey.Right2, 0f, lastFrameRight2));
			lastFrameLeft2 = true;
			lastFrameRight2 = false;
		}
		else if (x > 0f)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Right2, x, !lastFrameRight2));
			Notify(new InputEvent(Player, InputEvent.InputKey.Left2, 0f, lastFrameLeft2));
			lastFrameRight2 = true;
			lastFrameLeft2 = false;
		}
		else
		{
			if (lastFrameLeft2)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.Left2, 0f, changed: true));
			}
			if (lastFrameRight2)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.Right2, 0f, changed: true));
			}
			lastFrameLeft2 = false;
			lastFrameRight2 = false;
		}
		if (y2 > 0f)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Up2, y2, !lastFrameUp2));
			Notify(new InputEvent(Player, InputEvent.InputKey.Down2, 0f, lastFrameDown2));
			lastFrameUp2 = true;
			lastFrameDown2 = false;
		}
		else if (y2 < 0f)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Down2, 0f - y2, !lastFrameDown2));
			Notify(new InputEvent(Player, InputEvent.InputKey.Up2, 0f, lastFrameUp2));
			lastFrameDown2 = true;
			lastFrameUp2 = false;
		}
		else
		{
			if (lastFrameUp2)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.Up2, 0f, changed: true));
			}
			if (lastFrameDown2)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.Down2, 0f, changed: true));
			}
			lastFrameUp2 = false;
			lastFrameDown2 = false;
		}
		if (x != 0f || y2 != 0f)
		{
			if (Mathf.Abs(x) > orthoDirectionSelectionZone)
			{
				if (x > 0f)
				{
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoRight2, 1f, !lastFrameOrthoRight2));
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoLeft2, 0f, lastFrameOrthoLeft2));
					lastFrameOrthoRight2 = true;
					lastFrameOrthoLeft2 = false;
				}
				else
				{
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoLeft2, 1f, !lastFrameOrthoLeft2));
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoRight2, 0f, lastFrameOrthoRight2));
					lastFrameOrthoLeft2 = true;
					lastFrameOrthoRight2 = false;
				}
			}
			else if (Mathf.Abs(y2) > orthoDirectionSelectionZone)
			{
				if (y2 > 0f)
				{
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoUp2, 1f, !lastFrameOrthoUp2));
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoDown2, 0f, lastFrameOrthoDown2));
					lastFrameOrthoUp2 = true;
					lastFrameOrthoDown2 = false;
				}
				else
				{
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoDown2, 1f, !lastFrameOrthoDown2));
					Notify(new InputEvent(Player, InputEvent.InputKey.OrthoUp2, 0f, lastFrameOrthoUp2));
					lastFrameOrthoDown2 = true;
					lastFrameOrthoUp2 = false;
				}
			}
		}
		else
		{
			if (lastFrameOrthoUp2)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.OrthoUp2, 0f, changed: true));
			}
			if (lastFrameOrthoDown2)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.OrthoDown2, 0f, changed: true));
			}
			if (lastFrameOrthoLeft2)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.OrthoLeft2, 0f, changed: true));
			}
			if (lastFrameOrthoRight2)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.OrthoRight2, 0f, changed: true));
			}
			lastFrameOrthoUp2 = false;
			lastFrameOrthoDown2 = false;
			lastFrameOrthoLeft2 = false;
			lastFrameOrthoRight2 = false;
		}
		float value = device.LeftTrigger.Value;
		if (value > triggerDeadZone)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.LeftTrigger, value, !lastFrameLT));
			lastFrameLT = true;
		}
		else
		{
			if (lastFrameLT)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.LeftTrigger, 0f, changed: true));
			}
			lastFrameLT = false;
		}
		float value2 = device.RightTrigger.Value;
		if (value2 > triggerDeadZone)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.RightTrigger, value2, !lastFrameRT));
			lastFrameRT = true;
		}
		else
		{
			if (lastFrameRT)
			{
				Notify(new InputEvent(Player, InputEvent.InputKey.RightTrigger, 0f, changed: true));
			}
			lastFrameRT = false;
		}
		if (device.Action1.IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Jump, valueb: true, device.Action1.WasPressed));
			Notify(new InputEvent(Player, Action1MenuInputKey, valueb: true, device.Action1.WasPressed));
		}
		else if (device.Action1.WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Jump, valueb: false, changed: true));
			Notify(new InputEvent(Player, Action1MenuInputKey, valueb: false, changed: true));
		}
		if (device.Action2.IsPressed)
		{
			Notify(new InputEvent(Player, Action2MenuInputKey, valueb: true, device.Action2.WasPressed));
			Notify(new InputEvent(Player, InputEvent.InputKey.Suicide, valueb: true, device.Action2.WasPressed));
		}
		else if (device.Action2.WasReleased)
		{
			Notify(new InputEvent(Player, Action2MenuInputKey, valueb: false, changed: true));
			Notify(new InputEvent(Player, InputEvent.InputKey.Suicide, valueb: false, changed: true));
		}
		if (device.Action3.IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Sprint, valueb: true, device.Action3.WasPressed));
		}
		else if (device.Action3.WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Sprint, valueb: false, changed: true));
		}
		if (device.Action4.IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Inventory, valueb: true, device.Action4.WasPressed));
		}
		else if (device.Action4.WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Inventory, valueb: false, changed: true));
		}
		if (device.GetControl(InputControlType.Start).IsPressed || device.GetControl(InputControlType.Menu).IsPressed || device.GetControl(InputControlType.Options).IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Start, valueb: true, device.GetControl(InputControlType.Start).WasPressed || device.GetControl(InputControlType.Menu).WasPressed || device.GetControl(InputControlType.Options).WasPressed));
			Notify(new InputEvent(Player, InputEvent.InputKey.Pause, valueb: true, device.GetControl(InputControlType.Start).WasPressed || device.GetControl(InputControlType.Menu).WasPressed || device.GetControl(InputControlType.Options).WasPressed));
		}
		else if (device.GetControl(InputControlType.Start).WasReleased || device.GetControl(InputControlType.Menu).WasReleased || device.GetControl(InputControlType.Options).WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Start, valueb: false, changed: true));
			Notify(new InputEvent(Player, InputEvent.InputKey.Pause, valueb: false, changed: true));
		}
		if (device.GetControl(InputControlType.Back).IsPressed || device.GetControl(InputControlType.Select).IsPressed || device.GetControl(InputControlType.View).IsPressed || device.GetControl(InputControlType.Share).IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Scoreboard, valueb: true, device.GetControl(InputControlType.Back).WasPressed || device.GetControl(InputControlType.Select).WasPressed || device.GetControl(InputControlType.View).WasPressed || device.GetControl(InputControlType.Share).WasPressed));
		}
		else if (device.GetControl(InputControlType.Back).WasReleased || device.GetControl(InputControlType.Select).WasReleased || device.GetControl(InputControlType.View).WasReleased || device.GetControl(InputControlType.Share).WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.Scoreboard, valueb: false, changed: true));
		}
		if (device.LeftBumper.IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.RotateLeft, valueb: true, device.LeftBumper.WasPressed));
		}
		else if (device.LeftBumper.WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.RotateLeft, valueb: false, changed: true));
		}
		if (device.RightBumper.IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.RotateRight, valueb: true, device.RightBumper.WasPressed));
		}
		else if (device.RightBumper.WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.RotateRight, valueb: false, changed: true));
		}
		if (device.GetControl(InputControlType.DPadDown).IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.DpadDown, valueb: true, device.GetControl(InputControlType.DPadDown).WasPressed));
		}
		else if (device.GetControl(InputControlType.DPadDown).WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.DpadDown, valueb: false, changed: true));
		}
		if (device.GetControl(InputControlType.DPadUp).IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.DpadUp, valueb: true, device.GetControl(InputControlType.DPadUp).WasPressed));
		}
		else if (device.GetControl(InputControlType.DPadUp).WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.DpadUp, valueb: false, changed: true));
		}
		if (device.GetControl(InputControlType.DPadLeft).IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.DpadLeft, valueb: true, device.GetControl(InputControlType.DPadLeft).WasPressed));
		}
		else if (device.GetControl(InputControlType.DPadLeft).WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.DpadLeft, valueb: false, changed: true));
		}
		if (device.GetControl(InputControlType.DPadRight).IsPressed)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.DpadRight, valueb: true, device.GetControl(InputControlType.DPadRight).WasPressed));
		}
		else if (device.GetControl(InputControlType.DPadRight).WasReleased)
		{
			Notify(new InputEvent(Player, InputEvent.InputKey.DpadRight, valueb: false, changed: true));
		}
	}

	public override bool IsUsingPosition()
	{
		return false;
	}

	public override Vector2 GetVector(bool absolute = false)
	{
		return device.LeftStick.Vector;
	}

	public override void Reset()
	{
		associatedChars = new Character.Animals[4];
		Player = 0;
		assumeUser = false;
	}

	private static Vector2 CircleToSquare(Vector2 point)
	{
		return CircleToSquare(point, 0.0);
	}

	private static Vector2 CircleToSquare(Vector2 point, double innerRoundness)
	{
		double num = Math.Atan2(point.y, point.x) + 3.1415927410125732;
		Vector2 vector;
		if (num <= 0.7853981852531433 || num > 5.4977874755859375)
		{
			vector = point * (float)(1.0 / Math.Cos(num));
		}
		else if (num > 0.7853981852531433 && num <= 2.356194496154785)
		{
			vector = point * (float)(1.0 / Math.Sin(num));
		}
		else if (num > 2.356194496154785 && num <= 3.9269909858703613)
		{
			vector = point * (float)(-1.0 / Math.Cos(num));
		}
		else
		{
			if (!(num > 3.9269909858703613) || !(num <= 5.4977874755859375))
			{
				throw new InvalidOperationException("Invalid angle...?");
			}
			vector = point * (float)(-1.0 / Math.Sin(num));
		}
		if (innerRoundness == 0.0)
		{
			return vector;
		}
		float t = (float)Math.Pow(point.magnitude, innerRoundness);
		return Vector2.Lerp(point, vector, t);
	}
}
