using UnityEngine;

public class SawSpinAndMove : MonoBehaviour
{
	public float spinSpeed = 10f;

	public float moveSpeed = 10f;

	public bool goingLeft = true;

	public bool goingRight;

	public float maxLeft;

	public float maxRight = 6f;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, 0f, spinSpeed * Time.deltaTime), Space.Self);
		if (base.transform.position.x < maxLeft)
		{
			goingLeft = false;
		}
		if (base.transform.position.x > maxRight)
		{
			goingLeft = true;
		}
		if (goingLeft)
		{
			base.transform.Translate(new Vector3((0f - moveSpeed) * Time.deltaTime, 0f, 0f), Space.World);
		}
		else
		{
			base.transform.Translate(new Vector3(moveSpeed * Time.deltaTime, 0f, 0f), Space.World);
		}
	}
}
