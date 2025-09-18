using System.Collections;
using UnityEngine;

public class IceSpawner : MonoBehaviour
{
	public GameObject iceCube;

	public int timeDelay = 5;

	public float rangeXLowerLimit = -2f;

	public float rangeXUpperLimit = 2f;

	public float rangeYLowerLimit = -5f;

	public float rangeYUpperLimit = 5f;

	private void Start()
	{
		StartCoroutine(Example());
	}

	private IEnumerator Example()
	{
		for (int i = 0; i < 200; i++)
		{
			Vector2 spawnPoint = new Vector2(Random.Range(rangeXLowerLimit, rangeXUpperLimit), Random.Range(rangeYLowerLimit, rangeYUpperLimit));
			yield return new WaitForSeconds(timeDelay);
			Object.Instantiate(iceCube, spawnPoint, Quaternion.identity);
		}
	}

	private void Update()
	{
	}
}
