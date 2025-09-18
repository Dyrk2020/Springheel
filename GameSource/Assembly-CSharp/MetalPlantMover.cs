using System.Collections;
using UnityEngine;

public class MetalPlantMover : ActiveBlock
{
	public Animator[] movingBlockAnimators;

	public float movingSpeed = 1f;

	protected override void Awake()
	{
		base.Awake();
		SetOffsets();
	}

	protected override void Activate()
	{
		base.Activate();
		StartCoroutine(StartMovingDelay());
	}

	private IEnumerator StartMovingDelay()
	{
		yield return new WaitForSeconds(0.5f);
		for (int i = 0; i < movingBlockAnimators.Length; i++)
		{
			if (movingBlockAnimators[i] != null)
			{
				movingBlockAnimators[i].SetTrigger("Reset");
				movingBlockAnimators[i].SetFloat("Speed", movingSpeed);
			}
		}
		AkSoundEngine.PostEvent("SFX_Level_Metal_Plant_Start", base.gameObject);
	}

	public override void Reset()
	{
		base.Reset();
		Debug.Log("Resetting metal plant");
		for (int i = 0; i < movingBlockAnimators.Length; i++)
		{
			if (movingBlockAnimators[i] != null)
			{
				movingBlockAnimators[i].SetTrigger("Reset");
				movingBlockAnimators[i].SetFloat("Speed", 0f);
			}
		}
		AkSoundEngine.PostEvent("SFX_Level_Metal_Plant_Stop", base.gameObject);
	}

	protected override void ToPlaceMode(bool enableSelection)
	{
		base.ToPlaceMode(enableSelection);
		SetOffsets();
	}

	public override void Pause()
	{
		base.Pause();
		for (int i = 0; i < movingBlockAnimators.Length; i++)
		{
			if (movingBlockAnimators[i] != null)
			{
				movingBlockAnimators[i].speed = 0f;
			}
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		for (int i = 0; i < movingBlockAnimators.Length; i++)
		{
			if (movingBlockAnimators[i] != null)
			{
				movingBlockAnimators[i].speed = 1f;
			}
		}
	}

	private void SetOffsets()
	{
		float num = 1f / (float)movingBlockAnimators.Length;
		for (int i = 0; i < movingBlockAnimators.Length; i++)
		{
			if (movingBlockAnimators[i] != null)
			{
				movingBlockAnimators[i].SetFloat("Offset", num * (float)i);
				movingBlockAnimators[i].SetTrigger("Reset");
				movingBlockAnimators[i].Update(0.0001f);
			}
		}
	}

	protected override void EndPlay()
	{
		base.EndPlay();
		Reset();
	}
}
