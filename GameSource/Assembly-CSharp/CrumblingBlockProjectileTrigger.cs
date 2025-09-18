using UnityEngine;

public class CrumblingBlockProjectileTrigger : MonoBehaviour
{
	public CrumblingBlock crumblingblock;

	private void OnTriggerEnter2D(Collider2D collider)
	{
		crumblingblock.ProjectileTrigger();
	}
}
