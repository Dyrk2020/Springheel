using UnityEngine;

public class ScrapKiller : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.GetComponent<ScrapDropper>() != null)
		{
			Object.Destroy(col.gameObject);
		}
	}
}
