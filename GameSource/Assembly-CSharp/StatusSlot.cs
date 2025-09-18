using UnityEngine;
using UnityEngine.UI;

public class StatusSlot : MonoBehaviour
{
	public enum SlotState
	{
		HIDDEN,
		ALIVE,
		DEAD,
		RETRY,
		SUCCESS
	}

	public Image PlayerImage;

	public Image SuccessImage;

	public Image RetryImage;

	public Sprite Alive;

	public Sprite Dead;

	public void SetSlot(SlotState state)
	{
		PlayerImage.enabled = true;
		switch (state)
		{
		case SlotState.ALIVE:
			RetryImage.enabled = false;
			SuccessImage.enabled = false;
			PlayerImage.sprite = Alive;
			break;
		case SlotState.DEAD:
			RetryImage.enabled = false;
			SuccessImage.enabled = false;
			PlayerImage.sprite = Dead;
			break;
		case SlotState.RETRY:
			RetryImage.enabled = true;
			SuccessImage.enabled = false;
			break;
		case SlotState.SUCCESS:
			RetryImage.enabled = false;
			SuccessImage.enabled = true;
			PlayerImage.sprite = Alive;
			break;
		case SlotState.HIDDEN:
			RetryImage.enabled = false;
			SuccessImage.enabled = false;
			PlayerImage.enabled = false;
			break;
		}
	}
}
