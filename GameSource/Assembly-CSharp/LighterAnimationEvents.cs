using UnityEngine;

public class LighterAnimationEvents : MonoBehaviour
{
	public Lighter lighter;

	public void OnLighterOn()
	{
		lighter.SetLighterHazardState(onOff: true);
	}

	public void OnLighterOff()
	{
		lighter.SetLighterHazardState(onOff: false);
	}
}
