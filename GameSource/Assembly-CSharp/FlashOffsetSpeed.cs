using UnityEngine;

public class FlashOffsetSpeed : MonoBehaviour
{
	public float Offset;

	public float Speed;

	public Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	public void Show(bool show)
	{
		animator.SetBool("Show", show);
		if (show)
		{
			animator.SetFloat("Offset", Offset);
			animator.SetFloat("Speed", 1f / Speed);
		}
	}
}
