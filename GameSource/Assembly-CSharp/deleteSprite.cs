using UnityEngine;

public class deleteSprite : MonoBehaviour
{
	private void DeleteSprite()
	{
		Object.Destroy(base.gameObject.transform.parent.gameObject);
	}
}
