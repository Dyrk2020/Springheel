using UnityEngine;

public class ZeroAnchoredPositionOnStart : MonoBehaviour
{
	public void Start()
	{
		GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
	}
}
