public class TabletDisableGroup : TabletStyledObject
{
	public override void SetDisabled(bool disabled)
	{
		base.SetDisabled(disabled);
		TabletStyledObject[] componentsInChildren = GetComponentsInChildren<TabletStyledObject>(includeInactive: true);
		foreach (TabletStyledObject tabletStyledObject in componentsInChildren)
		{
			if (tabletStyledObject != this)
			{
				tabletStyledObject.SetDisabled(disabled);
			}
		}
	}
}
