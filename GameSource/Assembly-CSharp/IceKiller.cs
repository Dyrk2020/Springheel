using UnityEngine;

public class IceKiller : MonoBehaviour
{
	public GameObject iceCube;

	public GameObject iceKiller;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnCollisionEnter2D(Collision2D iceKiller)
	{
		Object.Destroy(iceCube);
	}
}
