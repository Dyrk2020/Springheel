using UnityEngine;

public class DanceLightGroup : MonoBehaviour
{
	protected Animator animator;

	protected SpriteRenderer[] srs;

	private void Awake()
	{
		srs = GetComponentsInChildren<SpriteRenderer>();
		animator = GetComponent<Animator>();
	}

	public void Activate(bool activate = true)
	{
		animator.SetBool("Enabled", activate);
	}

	public void Deactivate()
	{
		Activate(activate: false);
	}

	internal void Pause()
	{
		animator.speed = 0f;
	}

	internal void Unpause()
	{
		animator.speed = 1f;
	}
}
