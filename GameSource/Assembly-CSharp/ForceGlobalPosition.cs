using UnityEngine;

public class ForceGlobalPosition : MonoBehaviour
{
	public bool X;

	public bool Y;

	public bool Z;

	public Vector3 Position;

	private Vector3 fixedPosition;

	private void Update()
	{
		fixedPosition = base.transform.position;
		if (X)
		{
			fixedPosition.x = Position.x;
		}
		if (Y)
		{
			fixedPosition.y = Position.y;
		}
		if (Z)
		{
			fixedPosition.z = Position.z;
		}
		base.transform.position = fixedPosition;
	}
}
