using System.Collections.Generic;
using UnityEngine;

public class TabletPickableBlockButton : TabletButton
{
	public TabletBlock tabletBlock;

	private HashSet<PickCursor> clickingCursors = new HashSet<PickCursor>();

	public override void OnEnable()
	{
		base.OnEnable();
		if (tabletBlock.animator != null)
		{
			tabletBlock.animator.SetBool("Keep Active", value: false);
		}
	}

	public override void OnCursorOver()
	{
		base.OnCursorOver();
		if (!base.Disabled)
		{
			tabletBlock.OnCursorOver();
		}
	}

	public override void OnCursorOut()
	{
		base.OnCursorOut();
		tabletBlock.UpdateCrossoutAndResetAndFill();
		if (!base.Disabled)
		{
			tabletBlock.OnCursorOut();
		}
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		clickingCursors.Add(pickCursor);
		tabletBlock.SetProbability(getTargetProb());
		base.OnAccept(pickCursor);
	}

	public override void Update()
	{
		base.Update();
		if (tabletBlock.disabled || !base.HasTrackedCursors)
		{
			return;
		}
		if (clickingCursors.Count > 0)
		{
			foreach (PickCursor trackedCursor in trackedCursors)
			{
				if (clickingCursors.Contains(trackedCursor) && !trackedCursor.Held)
				{
					clickingCursors.Remove(trackedCursor);
				}
			}
		}
		if (clickingCursors.Count > 0)
		{
			int targetProb = getTargetProb();
			if (targetProb != tabletBlock.currentProbStep)
			{
				tabletBlock.SetProbability(targetProb);
			}
			return;
		}
		int targetProb2 = getTargetProb();
		if (targetProb2 == 0)
		{
			tabletBlock.IndicateBlockOff();
			return;
		}
		tabletBlock.IndicateBlockOn(targetProb2);
		tabletBlock.SetFillAmount(targetProb2);
		tabletBlock.SetCurrentFillAmount(tabletBlock.currentProbStep);
	}

	private int getTargetProb()
	{
		return Mathf.Clamp(Mathf.FloorToInt(GetNormalizedYPositionInContainer(tabletBlock.clickAreaRect, base.AverageTrackedCursorPosition) * (float)TabletBlock.buttonSections), 0, TabletBlock.buttonSections - 1);
	}

	public static float getVisualFill(int inputValue)
	{
		return (float)(inputValue + 1) / (float)TabletBlock.buttonSections;
	}
}
