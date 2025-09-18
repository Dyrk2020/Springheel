using UnityEngine;
using UnityEngine.UI;

public class MultiControllerButton : MonoBehaviour
{
	public InputEvent.InputKey inputKey;

	public Image buttonImage;

	public Image buttonBackgroundImage;

	public Text buttonText;

	public bool forceControllerType;

	public MultiControllerUIManager.ControllerType forcedControllerType;

	public bool preferMouseButtons;

	public bool hidden;

	public CanvasGroup canvasGroup;

	public bool isJoinIndicator;

	public InputEvent.InputKey lastUpdateInputKey = InputEvent.InputKey.NoKey;

	public MultiControllerUIManager.ControllerType lastUpdateControllerType = MultiControllerUIManager.ControllerType.KeyboardAndMouseAlt;

	public bool lastHiddenState;

	public bool firstUpdate;

	public bool Hidden
	{
		get
		{
			return hidden;
		}
		set
		{
			if (hidden != value)
			{
				hidden = value;
				MarkDirty();
			}
		}
	}

	private void Start()
	{
		if (Hidden)
		{
			buttonImage.enabled = false;
			buttonText.enabled = false;
			if (buttonBackgroundImage != null)
			{
				buttonBackgroundImage.enabled = false;
			}
		}
	}

	private void Update()
	{
		if (Hidden)
		{
			if (buttonImage.enabled)
			{
				buttonImage.enabled = false;
			}
			if (buttonText.enabled)
			{
				buttonText.enabled = false;
			}
			if (buttonBackgroundImage != null && buttonBackgroundImage.enabled)
			{
				buttonBackgroundImage.enabled = false;
			}
		}
		else
		{
			MultiControllerUIManager.Instance.UpdateButton(this);
		}
	}

	public void ForceController(Controller controller)
	{
		if (controller != null)
		{
			forceControllerType = true;
			forcedControllerType = MultiControllerUIManager.GetControllerType(controller);
		}
		else
		{
			forceControllerType = false;
		}
	}

	public void SetImageSprite(Sprite sprite)
	{
		if (sprite != null && !Hidden && !buttonImage.enabled)
		{
			buttonImage.enabled = true;
		}
		if (buttonImage.sprite != sprite)
		{
			buttonImage.sprite = sprite;
		}
		if (buttonBackgroundImage != null && buttonBackgroundImage.sprite != sprite)
		{
			buttonBackgroundImage.sprite = sprite;
		}
	}

	public void MarkDirty()
	{
		firstUpdate = false;
	}
}
