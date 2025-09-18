using UnityEngine;

public class SmokeEffect : MonoBehaviour
{
	private Animator animator;

	[HideInInspector]
	public SpriteRenderer SpriteRenderer;

	public Vector3 PositionOffset;

	public Vector3 DefaultScale;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		SpriteRenderer = GetComponent<SpriteRenderer>();
		SpriteRenderer.enabled = false;
		animator.enabled = false;
	}

	public void Poof()
	{
		SpriteRenderer.enabled = true;
		animator.enabled = true;
		animator.SetTrigger("Poof");
	}

	private void donePoofing()
	{
		SpriteRenderer.enabled = false;
		animator.enabled = false;
		base.transform.localPosition = Vector3.zero;
	}
}
