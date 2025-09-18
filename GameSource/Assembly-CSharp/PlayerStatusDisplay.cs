using System.Collections;
using UnityEngine;

public class PlayerStatusDisplay : MonoBehaviour
{
	public StatusSlot[] Slots;

	public Canvas IconCanvas;

	public CanvasGroup CanvasGroup;

	public float FadeTime = 0.1f;

	public void SetupSlot(int PlayerNumber, Sprite AliveSprite, Sprite DeadSprite)
	{
		StatusSlot obj = Slots[PlayerNumber - 1];
		obj.Alive = AliveSprite;
		obj.Dead = DeadSprite;
		SetSlot(PlayerNumber, StatusSlot.SlotState.ALIVE);
	}

	public void SetSlot(int playerNumber, StatusSlot.SlotState newState)
	{
		Slots[playerNumber - 1].SetSlot(newState);
	}

	public void HideAllSlots()
	{
		StatusSlot[] slots = Slots;
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i].SetSlot(StatusSlot.SlotState.HIDDEN);
		}
	}

	public void SetSlotCount(int count)
	{
		if (count > 4)
		{
			count = 4;
		}
		if (count < 0)
		{
			count = 0;
		}
		for (int i = count; i < Slots.Length; i++)
		{
			Slots[i].gameObject.SetActive(value: false);
		}
		for (int j = 0; j < count; j++)
		{
			Slots[j].gameObject.SetActive(value: true);
		}
	}

	public void SetAlpha(float alpha)
	{
		CanvasGroup.alpha = alpha;
	}

	public void FadeToAlpha(float targetAlpha)
	{
		StartCoroutine(FadeAlphaToCourotine(targetAlpha));
	}

	private IEnumerator FadeAlphaToCourotine(float targetAlpha)
	{
		while (!Mathf.Approximately(CanvasGroup.alpha, targetAlpha))
		{
			CanvasGroup.alpha = Mathf.MoveTowards(CanvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime / FadeTime);
			yield return null;
		}
		CanvasGroup.alpha = targetAlpha;
	}
}
