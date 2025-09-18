using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabletPointWidget : MonoBehaviour
{
	public TabletRulesScreen rulesScreen;

	public PointBlock.pointBlockType pointType;

	public RectTransform blockImageContainer;

	public Image blockImage;

	public TabletTextLabel blockName;

	public Image crossedOutImage;

	public TabletDisableGroup alwaysAwardCheckboxDisableGroup;

	public TabletCheckbox alwaysAwardCheckbox;

	public TabletButton pointBarButton;

	public Image pointBarColorGhost;

	private bool colorGhostShown = true;

	private int currentPointValue;

	public string validateSound;

	public RectTransform wobbleTarget;

	private IEnumerator anim;

	private HashSet<PickCursor> clickingCursors = new HashSet<PickCursor>();

	public void OnAlwaysAwardValueChange()
	{
		rulesScreen.SetPointAlwaysAward(pointType, alwaysAwardCheckbox.Value);
	}

	public void OnClickButton(PickCursor pickCursor)
	{
		rulesScreen.OnClickPointTypeEnabled(pointType);
	}

	public void OnClickPointArea(PickCursor pickCursor)
	{
		int pointValueAtPosition = GetPointValueAtPosition(pickCursor.cursorPoint.position);
		int num = GameSettings.GetInstance().PointTypeValue(pointType);
		if (pointValueAtPosition == num)
		{
			AkSoundEngine.PostEvent("UI_UPad_PointsSettings_Same", base.gameObject);
		}
		else if (pointValueAtPosition > num)
		{
			AkSoundEngine.PostEvent("UI_UPad_PointsSettings_Up", base.gameObject);
		}
		else
		{
			AkSoundEngine.PostEvent("UI_UPad_PointsSettings_Down", base.gameObject);
		}
		rulesScreen.SetPointValue(pointType, pointValueAtPosition);
		clickingCursors.Add(pickCursor);
		Wobble();
		if (!validateSound.NullOrEmpty())
		{
			AkSoundEngine.PostEvent(validateSound, base.gameObject);
		}
	}

	public void Wobble()
	{
		LocalScaleTweener localScaleTweener = new LocalScaleTweener(wobbleTarget, 1.1f.MakeVector3(), Vector3.one, 0.3f, Easings.Functions.CubicEaseOut);
		anim = localScaleTweener.PrimeAndAnimate();
	}

	private int GetPointValueAtPosition(Vector3 cursorWorldPosition)
	{
		Vector3[] array = new Vector3[4];
		blockImageContainer.GetWorldCorners(array);
		float num = array[2].x - array[0].x;
		int value = Mathf.RoundToInt(Mathf.Clamp01((cursorWorldPosition.x - array[0].x) / num) * 100f) / 10 * 10 + 10;
		GameSettings instance = GameSettings.GetInstance();
		return Mathf.Clamp(value, instance.minPointValue, instance.maxPointValue);
	}

	public void SetPointValue(int pointValue, bool ghost = false)
	{
		GameSettings instance = GameSettings.GetInstance();
		float num = (float)pointValue / (float)instance.maxPointValue;
		Vector2 sizeDelta = blockImageContainer.sizeDelta;
		sizeDelta.x *= num;
		if (ghost)
		{
			pointBarColorGhost.rectTransform.sizeDelta = sizeDelta;
			return;
		}
		currentPointValue = pointValue;
		blockImage.rectTransform.sizeDelta = sizeDelta;
	}

	public void SetPointEnabled(int val, bool animate = true)
	{
		if (pointType == PointBlock.pointBlockType.soloWin)
		{
			alwaysAwardCheckbox.gameObject.SetActive(value: false);
		}
		bool disabled = LobbyManager.instance != null && !LobbyManager.instance.IsHost;
		switch (val)
		{
		case 0:
			crossedOutImage.gameObject.SetActive(value: true);
			alwaysAwardCheckbox.SetValue(val: false, triggerCallback: false);
			alwaysAwardCheckboxDisableGroup.SetDisabled(disabled: true);
			pointBarButton.SetDisabled(disabled: true);
			blockImage.gameObject.SetActive(value: false);
			currentPointValue = 0;
			break;
		case 1:
			crossedOutImage.gameObject.SetActive(value: false);
			alwaysAwardCheckbox.SetValue(val: false, triggerCallback: false);
			if (pointType == PointBlock.pointBlockType.coin)
			{
				alwaysAwardCheckboxDisableGroup.SetDisabled(disabled: true);
			}
			else
			{
				alwaysAwardCheckboxDisableGroup.SetDisabled(disabled);
			}
			pointBarButton.SetDisabled(disabled);
			blockImage.gameObject.SetActive(value: true);
			break;
		case 2:
			crossedOutImage.gameObject.SetActive(value: false);
			alwaysAwardCheckbox.SetValue(val: true, triggerCallback: false);
			if (pointType == PointBlock.pointBlockType.coin)
			{
				alwaysAwardCheckboxDisableGroup.SetDisabled(disabled: true);
			}
			else
			{
				alwaysAwardCheckboxDisableGroup.SetDisabled(disabled);
			}
			pointBarButton.SetDisabled(disabled);
			blockImage.gameObject.SetActive(value: true);
			break;
		}
		if (val > 0)
		{
			int num = GameSettings.GetInstance().PointTypeValue(pointType);
			if (animate && currentPointValue != num)
			{
				Wobble();
			}
			SetPointValue(num);
		}
	}

	private void Update()
	{
		if (anim != null && !anim.MoveNext())
		{
			anim = null;
		}
		if (pointBarButton.HasTrackedCursors)
		{
			if (!colorGhostShown)
			{
				pointBarColorGhost.gameObject.SetActive(value: true);
				colorGhostShown = true;
			}
			int pointValueAtPosition = GetPointValueAtPosition(pointBarButton.AverageTrackedCursorPosition);
			if (clickingCursors.Count > 0)
			{
				HashSet<PickCursor> hashSet = new HashSet<PickCursor>();
				foreach (PickCursor trackedCursor in pointBarButton.trackedCursors)
				{
					if (!clickingCursors.Contains(trackedCursor))
					{
						continue;
					}
					if (trackedCursor.Held)
					{
						int pointValueAtPosition2 = GetPointValueAtPosition(trackedCursor.cursorPoint.position);
						if (currentPointValue != pointValueAtPosition2)
						{
							if (pointValueAtPosition2 > currentPointValue)
							{
								AkSoundEngine.PostEvent("UI_UPad_PointsSettings_Up", base.gameObject);
							}
							else
							{
								AkSoundEngine.PostEvent("UI_UPad_PointsSettings_Down", base.gameObject);
							}
							SetPointValue(pointValueAtPosition2);
						}
					}
					else
					{
						hashSet.Add(trackedCursor);
					}
				}
				foreach (PickCursor item in hashSet)
				{
					clickingCursors.Remove(item);
				}
			}
			SetPointValue(pointValueAtPosition, ghost: true);
			blockImage.gameObject.SetActive(pointValueAtPosition == currentPointValue);
		}
		if ((!pointBarButton.HasTrackedCursors || pointBarButton.Disabled) && colorGhostShown)
		{
			pointBarColorGhost.gameObject.SetActive(value: false);
			colorGhostShown = false;
			if (!pointBarButton.Disabled)
			{
				blockImage.gameObject.SetActive(value: true);
				Wobble();
			}
		}
	}
}
