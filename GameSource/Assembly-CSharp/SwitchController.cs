using UnityEngine;

public class SwitchController : Controller
{
	private static bool useAltButtonLayout;

	public static bool UseAltButtonLayout
	{
		get
		{
			return useAltButtonLayout;
		}
		set
		{
		}
	}

	public override Vector2 GetVector(bool absolute = false)
	{
		return Vector2.zero;
	}

	public override ControllerType GetControllerType()
	{
		return ControllerType.SWITCH_FULL;
	}

	public override bool IsUsingPosition()
	{
		return false;
	}

	public override void Reset()
	{
		associatedChars = new Character.Animals[4];
		Player = 0;
		assumeUser = false;
	}
}
