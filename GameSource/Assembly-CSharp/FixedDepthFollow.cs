using UnityEngine;

public class FixedDepthFollow : MonoBehaviour
{
	public float Depth;

	private void Update()
	{
		Vector3 position = ((!(base.transform.parent == null)) ? base.transform.parent.position : base.transform.position);
		position.z = Depth;
		base.transform.position = position;
	}
}
