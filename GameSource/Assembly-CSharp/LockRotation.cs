using UnityEngine;

public class LockRotation : MonoBehaviour
{
	private Quaternion initialWorldRotation;

	private void Awake()
	{
		initialWorldRotation = base.transform.rotation;
	}

	private void LateUpdate()
	{
		base.transform.rotation = initialWorldRotation;
	}
}
