using UnityEngine;

public class IceFall : MonoBehaviour
{
	public float fallSpeed = Random.Range(2f, 7f);

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Translate(Vector2.down * Time.deltaTime * fallSpeed);
	}
}
