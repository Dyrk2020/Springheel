using InControl;
using UnityEngine;

public abstract class InControlController : Controller
{
	public override int PadIndex => base.PadIndex;

	public abstract void SetInputDevice(InputDevice d);

	public abstract InputDevice GetInputDevice();

	public override ControllerType GetControllerType()
	{
		return ControllerType.GENERIC;
	}

	public override Vector2 GetVector(bool absolute = false)
	{
		return Vector2.zero;
	}

	public override bool IsUsingPosition()
	{
		return false;
	}

	public override void Reset()
	{
	}

	public void DebugOutputAllReceivers()
	{
		string text = "Receivers for controller " + GetInputDevice().Name + ":\n";
		foreach (InputReceiver receiver in receivers)
		{
			text = ((!(receiver is Object) || !((Object)receiver != null)) ? (text + "null\n") : (text + (receiver as MonoBehaviour).name + "\n"));
		}
		Debug.Log(text);
	}
}
