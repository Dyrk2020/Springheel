using UnityEngine;

namespace Crosstales.Common.Util;

public class RandomColor : MonoBehaviour
{
	[Tooltip("Use intervals to change the color (default: true).")]
	public bool UseInterval = true;

	[Tooltip("Random change interval between min (= x) and max (= y) in seconds (default: x = 5, y = 10).")]
	public Vector2 ChangeInterval = new Vector2(5f, 10f);

	[Tooltip("Random hue range between min (= x) and max (= y) (default: x = 0, y = 1).")]
	public Vector2 HueRange = new Vector2(0f, 1f);

	[Tooltip("Random saturation range between min (= x) and max (= y) (default: x = 1, y = 1).")]
	public Vector2 SaturationRange = new Vector2(1f, 1f);

	[Tooltip("Random value range between min (= x) and max (= y) (default: x = 1, y = 1).")]
	public Vector2 ValueRange = new Vector2(1f, 1f);

	[Tooltip("Random alpha range between min (= x) and max (= y) (default: x = 1, y = 1).")]
	public Vector2 AlphaRange = new Vector2(1f, 1f);

	[Tooltip("Use gray scale colors (default: false).")]
	public bool GrayScale;

	[Tooltip("Modify the color of a material instead of the Renderer (default: not set, optional).")]
	public Material Material;

	[Tooltip("Set the object to a random color at Start (default: false).")]
	public bool RandomColorAtStart;

	private float _elapsedTime;

	private float _changeTime;

	private Renderer _currentRenderer;

	private Color32 _startColor;

	private Color32 _endColor;

	private float _lerpProgress;

	private bool _existsMaterial;

	private static readonly int COLOR_ID = Shader.PropertyToID("_Color");

	private void Start()
	{
		_existsMaterial = Material != null;
		_elapsedTime = (_changeTime = Random.Range(ChangeInterval.x, ChangeInterval.y));
		if (RandomColorAtStart)
		{
			if (GrayScale)
			{
				float num = Random.Range(HueRange.x, HueRange.y);
				_startColor = new Color(num, num, num, Random.Range(AlphaRange.x, AlphaRange.y));
			}
			else
			{
				_startColor = Random.ColorHSV(HueRange.x, HueRange.y, SaturationRange.x, SaturationRange.y, ValueRange.x, ValueRange.y, AlphaRange.x, AlphaRange.y);
			}
			if (_existsMaterial)
			{
				Material.SetColor(COLOR_ID, _startColor);
				return;
			}
			_currentRenderer = GetComponent<Renderer>();
			_currentRenderer.material.color = _startColor;
		}
		else if (_existsMaterial)
		{
			_startColor = Material.GetColor(COLOR_ID);
		}
		else
		{
			_currentRenderer = GetComponent<Renderer>();
			_startColor = _currentRenderer.material.color;
		}
	}

	private void Update()
	{
		if (!UseInterval)
		{
			return;
		}
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime > _changeTime)
		{
			_lerpProgress = (_elapsedTime = 0f);
			if (GrayScale)
			{
				float num = Random.Range(HueRange.x, HueRange.y);
				_endColor = new Color(num, num, num, Random.Range(AlphaRange.x, AlphaRange.y));
			}
			else
			{
				_endColor = Random.ColorHSV(HueRange.x, HueRange.y, SaturationRange.x, SaturationRange.y, ValueRange.x, ValueRange.y, AlphaRange.x, AlphaRange.y);
			}
			_changeTime = Random.Range(ChangeInterval.x, ChangeInterval.y);
		}
		if (_existsMaterial)
		{
			Material.SetColor(COLOR_ID, Color.Lerp(_startColor, _endColor, _lerpProgress));
		}
		else
		{
			_currentRenderer.material.color = Color.Lerp(_startColor, _endColor, _lerpProgress);
		}
		if (_lerpProgress < 1f)
		{
			_lerpProgress += Time.deltaTime / (_changeTime - 0.1f);
		}
		else
		{
			_startColor = (_existsMaterial ? Material.GetColor(COLOR_ID) : _currentRenderer.material.color);
		}
	}
}
