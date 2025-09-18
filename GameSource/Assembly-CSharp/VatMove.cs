using System.Collections;
using UnityEngine;

public class VatMove : MonoBehaviour
{
	private bool movingUp = true;

	public float moveSpeed = 2f;

	public float upperLimit = 8f;

	public float lowerLimit = -8f;

	public float pauseTime = 3f;

	private void Start()
	{
	}

	private void Update()
	{
		if (movingUp)
		{
			StartCoroutine(Move());
		}
		else
		{
			StartCoroutine(Stop());
		}
	}

	private IEnumerator Move()
	{
		if (base.transform.position.y > upperLimit)
		{
			movingUp = false;
			yield break;
		}
		base.transform.Translate(Vector2.up * Time.deltaTime * moveSpeed);
		yield return new WaitForSeconds(0f);
	}

	private IEnumerator Stop()
	{
		yield return new WaitForSeconds(pauseTime);
		movingUp = true;
	}
}
