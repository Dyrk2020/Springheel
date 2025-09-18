using UnityEngine;

public class ControlDisplay : UIGraphic
{
	public bool ShowInventory;

	public bool ShowCancel;

	public bool ShowToRunMode;

	public SpriteRenderer YButton;

	public SpriteRenderer InventoryText;

	public override void Show()
	{
		base.Show();
		if (!ShowInventory)
		{
			YButton.enabled = false;
			InventoryText.enabled = false;
		}
	}
}
