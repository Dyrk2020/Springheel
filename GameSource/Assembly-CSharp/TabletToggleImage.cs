using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabletToggleImage : TabletStyledObject, TabletClickable
{
	public TabletTextLabel textLabel;

	public Image checkImage;

	public Sprite OnSprite;

	public Sprite OffSprite;

	public string tickSound = "UI_UPad_TickBox_Check";

	public string untickSound = "UI_UPad_TickBox_Uncheck";

	public string hoverSound = "UI_UPad_TickBox_Hover";

	public UnityEvent OnValueChange;

	[HideInInspector]
	public bool value;

	public bool Value => value;

	public void Awake()
	{
		SetValue(value, triggerCallback: false);
	}

	public void SetValue(bool val, bool triggerCallback = true)
	{
		value = val;
		if (value)
		{
			checkImage.sprite = OnSprite;
		}
		else
		{
			checkImage.sprite = OffSprite;
		}
		if (triggerCallback && OnValueChange != null)
		{
			OnValueChange.Invoke();
		}
	}

	public override void ResetStyles()
	{
		base.ResetStyles();
		if (textLabel != null)
		{
			textLabel.ResetStyles();
		}
		checkImage.color = colorScheme.mainTextColor;
	}

	public void OnAccept(PickCursor pickCursor)
	{
		if (Value)
		{
			if (!untickSound.NullOrEmpty())
			{
				AkSoundEngine.PostEvent(untickSound, base.gameObject);
			}
		}
		else if (!tickSound.NullOrEmpty())
		{
			AkSoundEngine.PostEvent(tickSound, base.gameObject);
		}
		SetValue(!value);
	}

	public void OnCursorOver()
	{
		if (!hoverSound.NullOrEmpty())
		{
			AkSoundEngine.PostEvent(hoverSound, base.gameObject);
		}
	}

	public void OnCursorOut()
	{
	}

	public override void SetDisabled(bool disabled)
	{
		base.SetDisabled(disabled);
		if (textLabel != null)
		{
			textLabel.SetDisabled(disabled);
		}
		if (checkImage != null)
		{
			checkImage.color = (base.Disabled ? colorScheme.mainTextColor_Disabled : colorScheme.mainTextColor);
		}
	}
}
