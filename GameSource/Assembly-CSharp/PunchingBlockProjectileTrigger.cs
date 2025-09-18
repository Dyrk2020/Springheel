using UnityEngine;

public class PunchingBlockProjectileTrigger : MonoBehaviour
{
	public PunchingBlock punchingBlock;

	public PunchingBlockTrigger punchingBlockTrigger;

	private void OnTriggerEnter2D(Collider2D collider)
	{
		punchingBlock.OnProjectileTouchedTrigger(punchingBlockTrigger);
	}
}
