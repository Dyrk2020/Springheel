using UnityEngine;

public class RaftDayNightHSVChanger : MonoBehaviour
{
	private static readonly int Color = Shader.PropertyToID("_Color");

	private static readonly int HueShiftAmount = Shader.PropertyToID("_HueShiftAmount");

	private static readonly int SatShiftAmount = Shader.PropertyToID("_SatShiftAmount");

	private static readonly int ValShiftAmount = Shader.PropertyToID("_ValShiftAmount");

	private static readonly int ContrastShiftAmount = Shader.PropertyToID("_ContrastShiftAmount");

	[SerializeField]
	private HSVColorData startColor = new HSVColorData();

	[SerializeField]
	private HSVColorData endColor = new HSVColorData();

	private SpriteRenderer renderer;

	private void Awake()
	{
		renderer = GetComponent<SpriteRenderer>();
	}

	public void HandleTransition(float progress)
	{
		if (renderer == null)
		{
			renderer = GetComponent<SpriteRenderer>();
		}
		Material material = renderer.material;
		material.SetColor(Color, UnityEngine.Color.Lerp(startColor.tint, endColor.tint, progress));
		material.SetFloat(HueShiftAmount, Mathf.Lerp(startColor.hue, endColor.hue, progress));
		material.SetFloat(SatShiftAmount, Mathf.Lerp(startColor.saturation, endColor.saturation, progress));
		material.SetFloat(ValShiftAmount, Mathf.Lerp(startColor.value, endColor.value, progress));
		material.SetFloat(ContrastShiftAmount, Mathf.Lerp(startColor.contrast, endColor.contrast, progress));
	}
}
