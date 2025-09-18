using UnityEngine;

public class WaveSystem : MonoBehaviour
{
	public GameObject frontwaves;

	public GameObject backwaves;

	public GameObject veryFrontWaves;

	protected SpriteRenderer[] frontWavesSR;

	protected SpriteRenderer[] BackwavesSR;

	protected SpriteRenderer[] VeryFrontSR;

	public Color frontWaveColor;

	public Color backWaveColor;

	public Color veryFrontWaveColor;

	protected Animator[] frontwaveAnimators;

	protected Animator[] backWaveAnimators;

	protected Animator[] veryFrontWavesAnimator;

	public float frontAnimatorSpeed;

	public float backAnimatorSpeed;

	public float veryFrontAnimatorSpeed;

	public bool updateAlways;

	public bool dontUpdateLayerOrder;

	private void Start()
	{
		frontWavesSR = frontwaves.GetComponentsInChildren<SpriteRenderer>();
		BackwavesSR = backwaves.GetComponentsInChildren<SpriteRenderer>();
		VeryFrontSR = veryFrontWaves.GetComponentsInChildren<SpriteRenderer>();
		frontwaveAnimators = frontwaves.GetComponentsInChildren<Animator>();
		backWaveAnimators = backwaves.GetComponentsInChildren<Animator>();
		veryFrontWavesAnimator = veryFrontWaves.GetComponentsInChildren<Animator>();
		updateEverything();
	}

	private void Update()
	{
		if (updateAlways)
		{
			updateEverything();
		}
	}

	private void updateEverything()
	{
		if (!dontUpdateLayerOrder)
		{
			SpriteRenderer[] array = frontWavesSR;
			foreach (SpriteRenderer obj in array)
			{
				obj.color = frontWaveColor;
				obj.sortingLayerName = "Foreground Background";
			}
			array = BackwavesSR;
			foreach (SpriteRenderer obj2 in array)
			{
				obj2.color = backWaveColor;
				obj2.sortingLayerName = "Background 1";
			}
			array = VeryFrontSR;
			foreach (SpriteRenderer obj3 in array)
			{
				obj3.color = veryFrontWaveColor;
				obj3.sortingOrder = 6;
				obj3.sortingLayerName = "Foreground Background";
			}
		}
		Animator[] array2 = frontwaveAnimators;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].speed = frontAnimatorSpeed;
		}
		array2 = backWaveAnimators;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].speed = backAnimatorSpeed;
		}
		array2 = veryFrontWavesAnimator;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].speed = veryFrontAnimatorSpeed;
		}
	}
}
