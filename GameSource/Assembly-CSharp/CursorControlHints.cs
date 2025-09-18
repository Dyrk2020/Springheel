using System.Collections.Generic;
using UnityEngine;

public class CursorControlHints : MonoBehaviour
{
	public enum Button
	{
		Inventory,
		Switch,
		Copy,
		PickUp,
		Cancel,
		Rotate,
		ShiftRotate
	}

	public CursorControlHintButton[] hintButtons;

	public CursorControlHintButton[] altHintButtons;

	public CursorControlHintButton[] crossButtons;

	public CursorControlHintButton[] circleButtons;

	private Dictionary<Button, CursorControlHintButton> buttonMap = new Dictionary<Button, CursorControlHintButton>();

	private void Awake()
	{
		CursorControlHintButton[] array = hintButtons;
		foreach (CursorControlHintButton cursorControlHintButton in array)
		{
			buttonMap.Add(cursorControlHintButton.button, cursorControlHintButton);
		}
		HideAll();
	}

	public void HideAll()
	{
		CursorControlHintButton[] array = hintButtons;
		foreach (CursorControlHintButton cursorControlHintButton in array)
		{
			if (cursorControlHintButton != null)
			{
				cursorControlHintButton.SetVisible(visible: false);
			}
		}
		array = altHintButtons;
		foreach (CursorControlHintButton cursorControlHintButton2 in array)
		{
			if (cursorControlHintButton2 != null)
			{
				cursorControlHintButton2.SetVisible(visible: false);
			}
		}
	}

	public void HideAll(List<CursorControlHintButton> DontHide)
	{
		CursorControlHintButton[] array = hintButtons;
		foreach (CursorControlHintButton cursorControlHintButton in array)
		{
			if (cursorControlHintButton != null && !DontHide.Contains(cursorControlHintButton))
			{
				cursorControlHintButton.SetVisible(visible: false);
			}
		}
		array = altHintButtons;
		foreach (CursorControlHintButton cursorControlHintButton2 in array)
		{
			if (cursorControlHintButton2 != null && !DontHide.Contains(cursorControlHintButton2))
			{
				cursorControlHintButton2.SetVisible(visible: false);
			}
		}
	}

	public void SetButtonVisible(Button button, bool visible, string textKey = null)
	{
		if (buttonMap.TryGetValue(button, out var value))
		{
			if (value != null)
			{
				value.SetVisible(visible, textKey);
			}
		}
		else
		{
			Debug.LogError("Could not find button " + button.ToString() + " in buttonMap");
		}
	}

	public CursorControlHintButton SetButtonVisibleReturn(Button button, bool visible, string textKey = null, bool highlighted = false)
	{
		if (buttonMap.TryGetValue(button, out var value))
		{
			if (value != null)
			{
				value.SetVisible(visible, textKey, highlighted);
				return value;
			}
		}
		else
		{
			Debug.LogError("Could not find button " + button.ToString() + " in buttonMap");
		}
		return null;
	}
}
