using UnityEngine;

public class PunchingPlantAnimator : MonoBehaviour
{
	private PunchingPlant mainScript;

	private void Start()
	{
		mainScript = GetComponentInParent<PunchingPlant>();
	}

	private void animPunch()
	{
		mainScript.punch();
	}

	private void animReset()
	{
		mainScript.reset();
	}
}
