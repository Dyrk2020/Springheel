using UnityEngine;

public class TabletInGameModsScreen : TabletScreen
{
	public TabletSimpleScroll settingsScroller;

	public override void OnCursorScroll(Vector2 scrollAmount)
	{
		settingsScroller.ApplyScrolling(scrollAmount.y);
	}

	public override bool OnRotateLeft(PickCursor pickCursor)
	{
		if (pickCursor.lastRotateWasMouseWheel)
		{
			if (Modifiers.GetInstance().CameraFlippedOnX)
			{
				settingsScroller.OnClickScrollPlus(pickCursor);
			}
			else
			{
				settingsScroller.OnClickScrollMinus(pickCursor);
			}
			return true;
		}
		return false;
	}

	public override bool OnRotateRight(PickCursor pickCursor)
	{
		if (pickCursor.lastRotateWasMouseWheel)
		{
			if (Modifiers.GetInstance().CameraFlippedOnX)
			{
				settingsScroller.OnClickScrollMinus(pickCursor);
			}
			else
			{
				settingsScroller.OnClickScrollPlus(pickCursor);
			}
			return true;
		}
		return false;
	}
}
