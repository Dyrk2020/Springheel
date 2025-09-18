using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerTweaks : MonoBehaviour
{
	public CanvasScaler target;

	public float nonSwitchDPPU = 1f;

	private void Awake()
	{
		target.dynamicPixelsPerUnit = nonSwitchDPPU;
	}
}
