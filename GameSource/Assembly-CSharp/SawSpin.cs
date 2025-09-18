using UnityEngine;

public class SawSpin : MonoBehaviour
{
	public float spinSpeed = 5f;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime, Space.Self);
	}
}
