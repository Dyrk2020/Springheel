using UnityEngine;

public class ScrapDropper : MonoBehaviour
{
	public float fallSpeed = -2f;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Translate(Vector2.down * Time.deltaTime * fallSpeed, Space.World);
		fallSpeed *= 1.01f;
		base.transform.Rotate(0f, 0f, 30f * Time.deltaTime, Space.Self);
	}

	private void OnCollisionEnter2D(Collision2D coll)
	{
	}
}
