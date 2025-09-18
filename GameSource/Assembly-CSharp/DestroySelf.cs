using UnityEngine;

public class DestroySelf : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Die()
	{
		Object.Destroy(base.gameObject);
	}
}
