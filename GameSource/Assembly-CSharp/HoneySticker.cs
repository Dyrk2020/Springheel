using UnityEngine;

public class HoneySticker : MonoBehaviour
{
	public Animator animator;

	public Transform StickyTarget;

	public bool followingCharacter;

	private float offset;

	private float scale;

	private void Start()
	{
		offset = Random.Range(-0.3f, 0.3f);
		scale = Random.Range(0.5f, 2.5f);
	}

	private void Update()
	{
		if (followingCharacter)
		{
			Vector3 vector = StickyTarget.position - base.transform.position;
			vector.x += offset;
			base.transform.localScale = new Vector3(1f, Mathf.Clamp(vector.magnitude * scale, 0.5f, 2.5f), 1f);
			vector.Normalize();
			float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			base.transform.rotation = Quaternion.Euler(0f, 0f, num - 90f);
		}
	}

	public void triggerHoneyStick(Transform stickyTarget)
	{
		StickyTarget = stickyTarget;
		followingCharacter = true;
		int value = Random.Range(0, 3);
		animator.SetInteger("Picker", value);
		animator.SetTrigger("StickTrigger");
	}
}
