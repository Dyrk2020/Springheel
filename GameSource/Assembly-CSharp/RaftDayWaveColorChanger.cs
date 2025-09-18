using UnityEngine;

[RequireComponent(typeof(WaveSystem))]
public class RaftDayWaveColorChanger : MonoBehaviour
{
	private WaveSystem _waveSystem;

	[SerializeField]
	private Color startFrontWaveColor;

	[SerializeField]
	private Color startBackWaveColor;

	[SerializeField]
	private Color startVeryFrontWaveColor;

	[SerializeField]
	private Color endFrontWaveColor = Color.white;

	[SerializeField]
	private Color endBackWaveColor = Color.white;

	[SerializeField]
	private Color endVeryFrontWaveColor = Color.white;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (!(_waveSystem != null))
		{
			_waveSystem = GetComponent<WaveSystem>();
		}
	}

	public void StartTransition()
	{
		Init();
		_waveSystem.updateAlways = true;
	}

	public void StopTransition()
	{
		_waveSystem.updateAlways = false;
	}

	public void HandleTransition(float progress)
	{
		Init();
		_waveSystem.frontWaveColor = Color.Lerp(startFrontWaveColor, endFrontWaveColor, progress);
		_waveSystem.backWaveColor = Color.Lerp(startBackWaveColor, endBackWaveColor, progress);
		_waveSystem.veryFrontWaveColor = Color.Lerp(startVeryFrontWaveColor, endVeryFrontWaveColor, progress);
	}
}
