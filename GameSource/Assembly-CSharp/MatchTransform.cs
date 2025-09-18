using UnityEngine;

public class MatchTransform : MonoBehaviour
{
	public Transform followTransform;

	public Vector2 OffsetVector;

	private void Start()
	{
	}

	private void Update()
	{
		if (followTransform != null)
		{
			base.transform.position = followTransform.position + (Vector3)OffsetVector;
		}
	}
}
