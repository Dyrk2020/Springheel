using System.Collections;
using UnityEngine;

public class ScrapSpawner : MonoBehaviour
{
	public GameObject scrapBlock;

	public int timeDelay = 5;

	private void Start()
	{
		StartCoroutine(Example());
	}

	private IEnumerator Example()
	{
		for (int i = 0; i < 200; i++)
		{
			Vector2 spawnPoint = new Vector2(37.5f, 30.5f);
			yield return new WaitForSeconds(timeDelay);
			Object.Instantiate(scrapBlock, spawnPoint, Quaternion.identity);
		}
	}

	private void Update()
	{
	}
}
