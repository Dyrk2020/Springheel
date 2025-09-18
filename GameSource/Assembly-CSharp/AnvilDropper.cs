using UnityEngine;

public class AnvilDropper : MonoBehaviour
{
	public Thwomp thwomp;

	[SerializeField]
	private Collider2D fieldOfViewCollider;

	private Animator animator;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	public void UpdateColliders(ThwompState state)
	{
		fieldOfViewCollider.enabled = state == ThwompState.REST;
	}

	public void PlayOpeningAnimation()
	{
		animator.SetBool("Opened", value: true);
	}

	public void PlayClosingAnimation()
	{
		animator.SetBool("Opened", value: false);
	}

	public void Pause()
	{
		animator.speed = 0f;
	}

	public void Unpause()
	{
		animator.speed = 1f;
	}

	public void Reset()
	{
		animator.SetBool("Opened", value: false);
	}

	public void OnOpeningComplete()
	{
		thwomp.OnDropperOpeningComplete();
	}
}
