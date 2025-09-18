using UnityEngine;

public class ToxicPoolManager : MonoBehaviour
{
	private static readonly int Opened = Animator.StringToHash("opened");

	private static readonly int Leaking = Animator.StringToHash("leaking");

	public GameObject[] poolBubbles;

	public Animator poolGateAnimator;

	public Animator toxicWasteBaseAnimator;

	public Animator toxicWasteWaterfallAnimator;

	public Animator valveAnimator;

	public Animator bubblesLeakingAnimator;

	private void Awake()
	{
		AkSoundEngine.PostEvent("SFX_Level_ToxicTower_Sludge_Start", base.gameObject);
	}

	public void DoReset()
	{
		poolGateAnimator.SetBool(Opened, value: false);
		valveAnimator.SetBool(Opened, value: false);
		toxicWasteBaseAnimator.SetBool(Leaking, value: false);
		GameObject[] array = poolBubbles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		toxicWasteWaterfallAnimator.SetBool(Leaking, value: false);
		bubblesLeakingAnimator.SetBool(Leaking, value: false);
	}

	private void OnDestroy()
	{
		AkSoundEngine.PostEvent("SFX_Level_ToxicTower_Sludge_Stop", base.gameObject);
	}

	public void StartPoolFillSound()
	{
		AkSoundEngine.PostEvent("SFX_Level_ToxicTower_Sludge_Fill", base.gameObject);
	}

	public void StartOpenGate()
	{
		AkSoundEngine.PostEvent("SFX_Level_ToxicTower_Gate_Open", base.gameObject);
		poolGateAnimator.SetBool(Opened, value: true);
		valveAnimator.SetBool(Opened, value: true);
	}

	public void StartWaterfallBase()
	{
		AkSoundEngine.PostEvent("SFX_Level_ToxicTower_Sludge_Fall", base.gameObject);
		toxicWasteBaseAnimator.SetBool(Leaking, value: true);
		GameObject[] array = poolBubbles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}

	public void StartWaterfall()
	{
		toxicWasteWaterfallAnimator.SetBool(Leaking, value: true);
		bubblesLeakingAnimator.SetBool(Leaking, value: true);
	}

	public void StartCloseGate()
	{
		AkSoundEngine.PostEvent("SFX_Level_ToxicTower_Gate_Close", base.gameObject);
		poolGateAnimator.SetBool(Opened, value: false);
		valveAnimator.SetBool(Opened, value: false);
	}

	public void StopBubblesLeaking()
	{
		GameObject[] array = poolBubbles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		bubblesLeakingAnimator.SetBool(Leaking, value: false);
	}

	public void StopWaterfall()
	{
		toxicWasteBaseAnimator.SetBool(Leaking, value: false);
		toxicWasteWaterfallAnimator.SetBool(Leaking, value: false);
	}
}
