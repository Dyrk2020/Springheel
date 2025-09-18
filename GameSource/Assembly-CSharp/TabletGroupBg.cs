using UnityEngine.UI;

public class TabletGroupBg : TabletStyledObject
{
	public override void ResetStyles()
	{
		base.ResetStyles();
		GetComponent<Image>().color = colorScheme.groupBgColor;
	}
}
