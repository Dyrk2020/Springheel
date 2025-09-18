using UnityEngine;

public class VatMoveUpDown : MonoBehaviour
{
	public float moveSpeed;

	public float lowerLimit;

	public float upperLimit;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Translate(Vector2.down * Time.deltaTime * moveSpeed);
		if (base.transform.position.y < lowerLimit)
		{
			moveSpeed = 0f - moveSpeed;
		}
		if (base.transform.position.y > upperLimit)
		{
			moveSpeed = 0f - moveSpeed;
		}
	}
}
