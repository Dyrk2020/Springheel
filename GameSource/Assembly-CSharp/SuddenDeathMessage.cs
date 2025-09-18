using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class SuddenDeathMessage : UIGraphic
{
	public Canvas TitleMessage;

	public Canvas InstructionMessage;

	public Canvas PressACanvas;

	public bool IsTimer;

	public bool WaitForPlayer;

	public float TimeToBeat = float.PositiveInfinity;

	public override void Show()
	{
		base.Show();
		Text[] componentsInChildren = InstructionMessage.GetComponentsInChildren<Text>();
		foreach (Text text in componentsInChildren)
		{
			if (IsTimer)
			{
				if (float.IsPositiveInfinity(TimeToBeat))
				{
					text.text = ScriptLocalization.InGameText.Fastest_time_to_the_End_Wins;
				}
				else
				{
					text.horizontalOverflow = HorizontalWrapMode.Overflow;
					text.verticalOverflow = VerticalWrapMode.Overflow;
					int num = Mathf.FloorToInt(TimeToBeat / 60f);
					float num2 = TimeToBeat - (float)(num * 60);
					text.text = ScriptLocalization.InGameText.Time_to_Beat + "\n" + num + ":" + ((num2 < 10f) ? "0" : "") + num2.ToString("F2");
				}
			}
			else
			{
				text.text = ScriptLocalization.InGameText.First_to_reach_the_flag_wins;
			}
			text.enabled = true;
		}
		if (WaitForPlayer)
		{
			PressACanvas.enabled = true;
		}
	}

	public override void Hide(bool forceQuickHide = false)
	{
		base.Hide(forceQuickHide);
		Text[] componentsInChildren = InstructionMessage.GetComponentsInChildren<Text>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		PressACanvas.enabled = false;
	}
}
