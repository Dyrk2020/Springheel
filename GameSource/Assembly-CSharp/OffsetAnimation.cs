using UnityEngine;

public class OffsetAnimation : MonoBehaviour
{
	public Animator animator;

	public float minOffset;

	public float maxOffset;

	private void Start()
	{
		animator.SetFloat("RandomOffset", Random.Range(minOffset, maxOffset));
	}
}
