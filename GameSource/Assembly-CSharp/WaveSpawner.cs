using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
	public GameObject WaveOfDoom;

	public Transform startingPoint;

	public float endPoint = 40f;

	public float startPoint = -81f;

	private void Start()
	{
	}

	private void Update()
	{
		if (WaveOfDoom.transform.position.x > endPoint)
		{
			WaveOfDoom.transform.Translate(new Vector3(startPoint, 0f, 0f), Space.World);
		}
	}
}
