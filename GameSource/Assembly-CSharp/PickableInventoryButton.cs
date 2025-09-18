using UnityEngine;

public class PickableInventoryButton : PickableButton
{
	public enum ButtonJobs
	{
		NEXTPAGE,
		PREVIOUSPAGE,
		CLOSEINVENTORY,
		ALLITEMS,
		NOITEMS,
		GAMEMODE
	}

	public ButtonJobs job;

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		switch (job)
		{
		case ButtonJobs.NEXTPAGE:
			inventoryBook.NextPage(1, allowClearBackPage: true, pageTurnSound: true);
			Controller.UnlockInputField();
			break;
		case ButtonJobs.PREVIOUSPAGE:
			inventoryBook.PreviousPage(allowClearBackPage: true, pageTurnSounds: true);
			Controller.UnlockInputField();
			break;
		case ButtonJobs.CLOSEINVENTORY:
			Controller.UnlockInputField();
			break;
		case ButtonJobs.ALLITEMS:
		case ButtonJobs.NOITEMS:
		case ButtonJobs.GAMEMODE:
			break;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!Visible || !initialized)
		{
			return;
		}
		if (!inventoryBook.ShowingOnHost && job != ButtonJobs.PREVIOUSPAGE && job != ButtonJobs.NEXTPAGE)
		{
			deactivatedInBook = true;
		}
		else
		{
			deactivatedInBook = false;
		}
		if (job == ButtonJobs.NEXTPAGE || job == ButtonJobs.PREVIOUSPAGE)
		{
			bool flag = true;
			if (job == ButtonJobs.NEXTPAGE && (pageNumber + 1 == inventoryBook.OptionPageNumber || pageNumber + 1 == inventoryBook.InventoryPages.Length))
			{
				flag = false;
			}
			if (job == ButtonJobs.PREVIOUSPAGE && pageNumber == 0)
			{
				flag = false;
			}
			if (image != null)
			{
				image.enabled = flag;
			}
			Collider2D[] pickColliders = PickColliders;
			for (int i = 0; i < pickColliders.Length; i++)
			{
				pickColliders[i].enabled = flag;
			}
		}
		if (deactivatedInBook)
		{
			if (overrideDeactivatedAlphaBool)
			{
				SetAlpha(overrideDeactivatedAlphafloat);
			}
			else
			{
				SetAlpha(0.5f);
			}
		}
		else
		{
			SetAlpha(1f);
		}
	}

	private void TurnNumberOnOrOff(bool on)
	{
	}
}
