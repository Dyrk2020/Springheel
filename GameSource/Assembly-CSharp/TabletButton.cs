using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TabletButton : TabletStyledObject, TabletClickable
{
	public enum ButtonType
	{
		Simple,
		Transparent,
		Invisible,
		Dangerous
	}

	public enum ClickAnimation
	{
		None,
		Wobble,
		Shade
	}

	public Image background;

	public Image labelImage;

	public string clickSound = "UI_UPad_Button_Click_Accept";

	public string hoverSound = "UI_UPad_Button_Hover";

	public TabletButtonEvent OnClick;

	public ButtonType buttonType;

	public bool tracksCursors;

	public bool tracksCursorsWhenDisabled;

	public ClickAnimation clickAnimation = ClickAnimation.Wobble;

	private IEnumerator clickAnim;

	public RectTransform toolTip;

	private bool showToolTip;

	public HashSet<PickCursor> trackedCursors = new HashSet<PickCursor>();

	public override bool TracksCursors => tracksCursors;

	public bool HasTrackedCursors
	{
		get
		{
			if (trackedCursors.Count > 0)
			{
				if (!tracksCursorsWhenDisabled)
				{
					return !disabled;
				}
				return true;
			}
			return false;
		}
	}

	public Vector3 AverageTrackedCursorPosition
	{
		get
		{
			switch (trackedCursors.Count)
			{
			case 0:
				return Vector3.zero;
			case 1:
				if (trackedCursors.FirstOrDefault() != null)
				{
					return trackedCursors.FirstOrDefault().cursorPoint.position;
				}
				return Vector3.zero;
			default:
			{
				Vector3 zero = Vector3.zero;
				int num = 0;
				foreach (PickCursor trackedCursor in trackedCursors)
				{
					zero += trackedCursor.cursorPoint.position;
					num++;
				}
				return zero / num;
			}
			}
		}
	}

	public void ClearTrackedCursors()
	{
		trackedCursors.Clear();
	}

	public virtual void OnCursorOver()
	{
		if (!base.Disabled && !hoverSound.NullOrEmpty())
		{
			AkSoundEngine.PostEvent(hoverSound, base.gameObject);
		}
		if (background != null)
		{
			switch (buttonType)
			{
			case ButtonType.Simple:
				background.color = (base.Disabled ? colorScheme.buttonBgColor_Disabled : colorScheme.buttonBgColor_Hover);
				break;
			case ButtonType.Transparent:
				background.color = (base.Disabled ? Color.clear : colorScheme.buttonBgColor_TransparentHighlight);
				break;
			case ButtonType.Invisible:
				background.color = Color.clear;
				break;
			case ButtonType.Dangerous:
				background.color = (base.Disabled ? colorScheme.buttonBgColor_Dangerous_Disabled : colorScheme.buttonBgColor_Dangerous_Hover);
				break;
			}
		}
	}

	public virtual void OnCursorOut()
	{
		if (background != null)
		{
			switch (buttonType)
			{
			case ButtonType.Simple:
				background.color = (base.Disabled ? colorScheme.buttonBgColor_Disabled : colorScheme.buttonBgColor);
				break;
			case ButtonType.Transparent:
				background.color = Color.clear;
				break;
			case ButtonType.Invisible:
				background.color = Color.clear;
				break;
			case ButtonType.Dangerous:
				background.color = (base.Disabled ? colorScheme.buttonBgColor_Dangerous_Disabled : colorScheme.buttonBgColor_Dangerous);
				break;
			}
		}
	}

	public virtual void OnEnable()
	{
		if (clickAnim != null)
		{
			clickAnim = null;
			if (clickAnimation == ClickAnimation.Wobble)
			{
				base.transform.localScale = Vector3.one;
			}
		}
		ClearTrackedCursors();
		OnCursorOut();
	}

	public override void ResetStyles()
	{
		base.ResetStyles();
		if (background != null)
		{
			switch (buttonType)
			{
			case ButtonType.Simple:
				background.color = colorScheme.buttonBgColor;
				break;
			case ButtonType.Transparent:
				background.color = Color.clear;
				break;
			case ButtonType.Invisible:
				background.color = Color.clear;
				break;
			case ButtonType.Dangerous:
				background.color = colorScheme.buttonBgColor_Dangerous;
				break;
			}
		}
		if (labelImage != null)
		{
			labelImage.color = colorScheme.mainTextColor;
		}
		TabletStyledObject[] componentsInChildren = GetComponentsInChildren<TabletStyledObject>(includeInactive: true);
		foreach (TabletStyledObject tabletStyledObject in componentsInChildren)
		{
			if (tabletStyledObject != this)
			{
				tabletStyledObject.colorScheme = colorScheme;
				tabletStyledObject.ResetStyles();
			}
		}
	}

	public override void SetDisabled(bool disabled)
	{
		base.SetDisabled(disabled);
		if (background != null)
		{
			switch (buttonType)
			{
			case ButtonType.Simple:
				background.color = (base.Disabled ? colorScheme.buttonBgColor_Disabled : colorScheme.buttonBgColor);
				break;
			case ButtonType.Transparent:
				background.color = Color.clear;
				break;
			case ButtonType.Invisible:
				background.color = Color.clear;
				break;
			case ButtonType.Dangerous:
				background.color = (base.Disabled ? colorScheme.buttonBgColor_Dangerous_Disabled : colorScheme.buttonBgColor_Dangerous);
				break;
			}
		}
		if (labelImage != null)
		{
			labelImage.color = (base.Disabled ? colorScheme.mainTextColor_Disabled : colorScheme.mainTextColor);
		}
		TabletTextLabel componentInChildren = GetComponentInChildren<TabletTextLabel>();
		if (componentInChildren != null)
		{
			componentInChildren.SetDisabled(disabled);
		}
		if (TracksCursors && !tracksCursorsWhenDisabled && disabled)
		{
			ClearTrackedCursors();
		}
	}

	public virtual void OnAccept(PickCursor pickCursor)
	{
		if (clickAnimation != ClickAnimation.None)
		{
			AnimateClick();
		}
		if (!clickSound.NullOrEmpty())
		{
			AkSoundEngine.PostEvent(clickSound, base.gameObject);
		}
		if (OnClick != null)
		{
			OnClick.Invoke(pickCursor);
		}
	}

	public override void AddTrackedCursor(PickCursor pickCursor)
	{
		if (TracksCursors)
		{
			trackedCursors.Add(pickCursor);
		}
	}

	public override void RemoveTrackedCursor(PickCursor pickCursor)
	{
		if (TracksCursors)
		{
			trackedCursors.Remove(pickCursor);
		}
	}

	public float GetNormalizedXPositionInContainer(RectTransform container, Vector3 cursorWorldPosition)
	{
		Vector3[] array = new Vector3[4];
		container.GetWorldCorners(array);
		float num = array[2].x - array[0].x;
		return Mathf.Clamp01((cursorWorldPosition.x - array[0].x) / num);
	}

	public float GetNormalizedYPositionInContainer(RectTransform container, Vector3 cursorWorldPosition)
	{
		Vector3[] array = new Vector3[4];
		container.GetWorldCorners(array);
		float num = array[2].y - array[0].y;
		return Mathf.Clamp01((cursorWorldPosition.y - array[0].y) / num);
	}

	public virtual void Update()
	{
		if (clickAnim != null && !clickAnim.MoveNext())
		{
			clickAnim = null;
		}
		if (!(toolTip != null))
		{
			return;
		}
		if (HasTrackedCursors)
		{
			if (!showToolTip)
			{
				showToolTip = true;
				toolTip.gameObject.SetActive(value: true);
			}
		}
		else if (showToolTip)
		{
			showToolTip = false;
			toolTip.gameObject.SetActive(value: false);
		}
	}

	private void AnimateClick()
	{
		switch (clickAnimation)
		{
		case ClickAnimation.Shade:
			if (background != null)
			{
				ImageColorTweener imageColorTweener = new ImageColorTweener(background, colorScheme.buttonBgColor_Disabled, colorScheme.buttonBgColor, 0.25f, Easings.Functions.CubicEaseOut);
				clickAnim = imageColorTweener.PrimeAndAnimate();
			}
			break;
		case ClickAnimation.Wobble:
		{
			LocalScaleTweener localScaleTweener = new LocalScaleTweener(base.transform, 1.1f.MakeVector3(), Vector3.one, 0.25f, Easings.Functions.CubicEaseOut);
			clickAnim = localScaleTweener.PrimeAndAnimate();
			break;
		}
		}
	}
}
