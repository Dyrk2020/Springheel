using UnityEngine;

public class RescaleFeetPhysicsSensor : MonoBehaviour
{
	public BoxCollider2D boxCollider;

	public float minYSize = 0.1f;

	private void LateUpdate()
	{
		if (!boxCollider.enabled)
		{
			return;
		}
		if (Modifiers.GetInstance().CharacterRelativeScale < 1f)
		{
			float num = boxCollider.size.y * base.transform.lossyScale.y;
			if (num < minYSize)
			{
				float num2 = minYSize / num;
				boxCollider.size = new Vector2(boxCollider.size.x, minYSize * num2 + 0.001f);
			}
		}
		else if (boxCollider.size.y != minYSize)
		{
			boxCollider.size = new Vector2(boxCollider.size.x, minYSize);
		}
	}
}
