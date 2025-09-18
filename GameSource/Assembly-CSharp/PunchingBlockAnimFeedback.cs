using UnityEngine;

public class PunchingBlockAnimFeedback : MonoBehaviour
{
	public PunchingBlock punchingBlock;

	public void OnPunchAnimFinished()
	{
		punchingBlock.OnPunchAnimFinished();
	}

	public void EnableHazard()
	{
		punchingBlock.EnableHazard();
	}

	public void DisableHazard()
	{
		punchingBlock.DisableHazard();
	}
}
