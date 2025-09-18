public class HoldableButton : PickableButton
{
	public bool Held
	{
		get
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return false;
			}
			foreach (Cursor hoveredCursor in HoveredCursors)
			{
				if (hoveredCursor.Held)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool HeldWithSprint
	{
		get
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return false;
			}
			foreach (Cursor hoveredCursor in HoveredCursors)
			{
				if (hoveredCursor.Held && hoveredCursor.Sprinting)
				{
					return true;
				}
			}
			return false;
		}
	}
}
