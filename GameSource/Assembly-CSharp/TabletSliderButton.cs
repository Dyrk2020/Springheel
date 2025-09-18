using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabletSliderButton : TabletButton
{
	public RectTransform sliderArea;

	public RectTransform barArea;

	public Image sliderImage;

	public Image markerImage;

	public float Value;

	private HashSet<PickCursor> clickingCursors = new HashSet<PickCursor>();

	public UnityEvent OnValueChange;

	private bool hovered;

	public override void OnAccept(PickCursor pickCursor)
	{
		float normalizedXPositionInContainer = GetNormalizedXPositionInContainer(barArea, pickCursor.cursorPoint.position);
		SetValue(normalizedXPositionInContainer, sendEvent: true);
		AkSoundEngine.PostEvent("UI_UPad_Slider_Click", base.gameObject);
		clickingCursors.Add(pickCursor);
	}

	private void SetMarkerPosition(float val)
	{
		markerImage.rectTransform.localPosition = new Vector3(barArea.rect.width * val, 0f);
	}

	private void SetSliderPosition(float val)
	{
		sliderImage.rectTransform.localPosition = new Vector3(barArea.rect.width * val, 0f);
	}

	public void SetValue(float val, bool sendEvent)
	{
		Value = val;
		SetSliderPosition(val);
		SetMarkerPosition(val);
		if (sendEvent)
		{
			OnValueChange.Invoke();
		}
	}

	public override void OnCursorOut()
	{
		base.OnCursorOut();
		if (hovered && !base.HasTrackedCursors)
		{
			SetHovered(val: false);
		}
		SetValue(Value, sendEvent: false);
	}

	private void SetHovered(bool val)
	{
		if (hovered != val)
		{
			hovered = val;
			if (val)
			{
				sliderImage.transform.localScale = 1.1f.MakeVector3();
			}
			else
			{
				sliderImage.transform.localScale = Vector3.one;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (clickingCursors.Count > 0)
		{
			if (!base.HasTrackedCursors)
			{
				clickingCursors.Clear();
			}
			else
			{
				HashSet<PickCursor> hashSet = new HashSet<PickCursor>();
				foreach (PickCursor trackedCursor in trackedCursors)
				{
					if (clickingCursors.Contains(trackedCursor) && !trackedCursor.Held)
					{
						hashSet.Add(trackedCursor);
					}
				}
				foreach (PickCursor item in hashSet)
				{
					clickingCursors.Remove(item);
				}
			}
		}
		if (!base.HasTrackedCursors)
		{
			return;
		}
		float normalizedXPositionInContainer = GetNormalizedXPositionInContainer(barArea, base.AverageTrackedCursorPosition);
		if (clickingCursors.Count > 0)
		{
			SetValue(normalizedXPositionInContainer, sendEvent: true);
			if (hovered)
			{
				SetHovered(val: false);
			}
		}
		else if (!hovered)
		{
			SetHovered(val: true);
		}
	}
}
