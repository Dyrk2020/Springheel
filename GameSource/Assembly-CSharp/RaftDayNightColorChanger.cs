using UnityEngine;

public class RaftDayNightColorChanger : MonoBehaviour
{
	private SpriteRenderer[] renderers;

	[SerializeField]
	private Color startColor = Color.white;

	[SerializeField]
	private Color endColor = Color.white;

	private void Awake()
	{
		if (renderers == null)
		{
			renderers = GetComponentsInChildren<SpriteRenderer>();
		}
		SpriteRenderer[] array = renderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = startColor;
		}
	}

	public void HandleTransition(float progress)
	{
		if (renderers == null)
		{
			renderers = GetComponentsInChildren<SpriteRenderer>();
		}
		SpriteRenderer[] array = renderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = Color.Lerp(startColor, endColor, progress);
		}
	}
}
