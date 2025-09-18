using System.Collections;
using UnityEngine;

public class RaftDayNightCycle : MonoBehaviour
{
	private static RaftDayNightCycle _instance;

	[SerializeField]
	private SpriteRenderer skyRenderer;

	[SerializeField]
	private Gradient skyGradient;

	[SerializeField]
	private float transitionTime = 1f;

	public float dayNightProgress;

	private RaftDayNightColorChanger[] _colorChangers;

	private RaftDayNightHSVChanger[] _hsvChangers;

	private RaftDayWaveColorChanger _waveColorChangers;

	private bool setupped;

	public static RaftDayNightCycle instance => _instance;

	protected void Awake()
	{
		_instance = this;
		AkSoundEngine.PostEvent("SFX_Level_Islands_DayTime", base.gameObject);
		Init();
	}

	private void Init()
	{
		if (!setupped)
		{
			setupped = true;
			_colorChangers = Object.FindObjectsOfType<RaftDayNightColorChanger>();
			_hsvChangers = Object.FindObjectsOfType<RaftDayNightHSVChanger>();
			_hsvChangers = Object.FindObjectsOfType<RaftDayNightHSVChanger>();
			_waveColorChangers = Object.FindObjectOfType<RaftDayWaveColorChanger>();
		}
	}

	public void NightTransition()
	{
		AkSoundEngine.PostEvent("SFX_Level_Islands_NightTime", base.gameObject);
		StartCoroutine(DoTransition(0f, 1f));
	}

	public void DayTransition()
	{
		AkSoundEngine.PostEvent("SFX_Level_Islands_DayTime", base.gameObject);
		StartCoroutine(DoTransition(1f, 0f));
	}

	private IEnumerator DoTransition(float start, float end)
	{
		Init();
		_waveColorChangers.StartTransition();
		float time = 0f;
		while (time < transitionTime)
		{
			time += Time.deltaTime;
			dayNightProgress = Mathf.Lerp(start, end, time / transitionTime);
			UpdateColors();
			yield return null;
		}
		_waveColorChangers.StopTransition();
	}

	private void UpdateColors()
	{
		skyRenderer.color = skyGradient.Evaluate(dayNightProgress);
		RaftDayNightColorChanger[] colorChangers = _colorChangers;
		for (int i = 0; i < colorChangers.Length; i++)
		{
			colorChangers[i].HandleTransition(dayNightProgress);
		}
		RaftDayNightHSVChanger[] hsvChangers = _hsvChangers;
		for (int i = 0; i < hsvChangers.Length; i++)
		{
			hsvChangers[i].HandleTransition(dayNightProgress);
		}
		_waveColorChangers.HandleTransition(dayNightProgress);
	}
}
