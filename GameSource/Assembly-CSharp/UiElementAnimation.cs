using UnityEngine;

public class UiElementAnimation : MonoBehaviour
{
	public Animator animator;

	public void Reset()
	{
		animator.SetBool("Show", value: false);
	}

	public void Activate()
	{
		animator.SetBool("Show", value: true);
	}
}
