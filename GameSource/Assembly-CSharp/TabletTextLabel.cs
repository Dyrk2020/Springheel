using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class TabletTextLabel : TabletStyledObject
{
	public enum LabelType
	{
		Normal,
		Title,
		Subtitle,
		ButtonText,
		SmallText,
		LargeText,
		TinyText,
		CustomSize,
		CustomSize_Modified
	}

	public LabelType labelType;

	public bool dynamicTextSize;

	public int smallSizeText = 30;

	public int mediumSizeText = 40;

	public int bigSizeText = 50;

	public int smallCharacterThreshold = 8;

	public int mediumCharacterThreshold = 6;

	public string text
	{
		get
		{
			return GetComponent<Text>().text;
		}
		set
		{
			GetComponent<Text>().text = value;
		}
	}

	public string Term
	{
		set
		{
			GetComponent<Localize>().Term = value;
		}
	}

	public bool EnableWordFilter
	{
		get
		{
			WordFilter component = GetComponent<WordFilter>();
			if (component != null)
			{
				return component.enabled;
			}
			return false;
		}
		set
		{
			WordFilter component = GetComponent<WordFilter>();
			if (component != null)
			{
				component.enabled = value;
			}
		}
	}

	public override void ResetStyles()
	{
		base.ResetStyles();
		UpdateTextColorAndSize();
	}

	public override void SetDisabled(bool disabled)
	{
		base.SetDisabled(disabled);
		UpdateTextColorAndSize();
	}

	public void UpdateTextColorAndSize()
	{
		Text component = GetComponent<Text>();
		if (colorScheme == null)
		{
			Debug.LogError("Missing Color Scheme on object:" + base.gameObject.name);
			return;
		}
		switch (labelType)
		{
		case LabelType.CustomSize:
			component.color = (base.Disabled ? colorScheme.mainTextColor_Disabled : colorScheme.mainTextColor);
			break;
		case LabelType.CustomSize_Modified:
			component.color = (base.Disabled ? colorScheme.mainTextColor_Modified_Disabled : colorScheme.mainTextColor_Modified);
			break;
		default:
			component.color = (base.Disabled ? colorScheme.mainTextColor_Disabled : colorScheme.mainTextColor);
			component.fontSize = colorScheme.mainTextSize;
			break;
		case LabelType.SmallText:
			component.color = (base.Disabled ? colorScheme.mainTextColor_Disabled : colorScheme.mainTextColor);
			component.fontSize = colorScheme.mainTextSmallSize;
			break;
		case LabelType.TinyText:
			component.color = (base.Disabled ? colorScheme.mainTextColor_Disabled : colorScheme.mainTextColor);
			component.fontSize = colorScheme.mainTextTinySize;
			break;
		case LabelType.LargeText:
			component.color = (base.Disabled ? colorScheme.mainTextColor_Disabled : colorScheme.mainTextColor);
			component.fontSize = colorScheme.mainTextLargeSize;
			break;
		case LabelType.Title:
			component.color = (base.Disabled ? colorScheme.mainTextColor_Disabled : colorScheme.mainTextColor);
			component.fontSize = colorScheme.titleTextSize;
			break;
		case LabelType.Subtitle:
			component.color = (base.Disabled ? colorScheme.subtitleColor_Disabled : colorScheme.subtitleColor);
			component.fontSize = colorScheme.subtitleTextSize;
			break;
		}
	}

	public void UpdateDynamicText()
	{
		Text component = GetComponent<Text>();
		if (!(component != null) || !dynamicTextSize)
		{
			return;
		}
		if (component.text.Length > mediumCharacterThreshold)
		{
			if (component.text.Length > smallCharacterThreshold)
			{
				component.fontSize = smallSizeText;
			}
			else
			{
				component.fontSize = mediumSizeText;
			}
		}
		else
		{
			component.fontSize = bigSizeText;
		}
	}
}
