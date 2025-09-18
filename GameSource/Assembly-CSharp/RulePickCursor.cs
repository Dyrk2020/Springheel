using GameEvent;
using UnityEngine.Networking;

public class RulePickCursor : PickCursor
{
	public override void Start()
	{
		base.Start();
		SetLayer(5);
	}

	public override void dealWithPickable()
	{
		PickableBlock pickableBlock = lastHoveredPick as PickableBlock;
		if (pickableBlock.inventoryBook != null && pickableBlock.inventoryBook.ShowingOnHost)
		{
			if (pickableBlock.Available)
			{
				pickableBlock.DeactivateItem();
			}
			else
			{
				pickableBlock.ActivateItem();
			}
		}
	}

	protected override void OnInventory()
	{
		if (!InventoryBookMenu.ScreenMode || !(InventoryBookMenu.CurrentScreenpage == InventoryBookMenu.TabletPage) || GameSettings.GetInstance().AvailableBlocks > 0)
		{
			if (InventoryBookMenu.FrozenOnPage)
			{
				return;
			}
			if (InventoryBookMenu.CurrentScreenpage != null)
			{
				if (InventoryBookMenu.CurrentScreenpage == InventoryBookMenu.SecondScreenPage)
				{
					UndergroundComputer component = InventoryBookMenu.SecondScreenPage.GetComponent<UndergroundComputer>();
					if (component.IsInSubmenu || DropdownMenu.dropdownDeployed)
					{
						component.BackOutOfSubmenu();
						return;
					}
				}
				if (InventoryBookMenu.CurrentScreenpage == InventoryBookMenu.TabletPage)
				{
					Tablet component2 = InventoryBookMenu.TabletPage.GetComponent<Tablet>();
					if (component2 != null && component2.OnPressBack(this))
					{
						return;
					}
				}
				if (InventoryBookMenu.CurrentScreenpage.ScreenBackButtonTarget != InventoryPage.PageTypes.nonePage && InventoryBookMenu.ScreenMode && PickableNetworkButton.backButton != null)
				{
					PickableNetworkButton.backButton.OnAccept(this);
					return;
				}
			}
			Player player = ((LocalPlayer != null) ? LocalPlayer : PlayerManager.GetInstance().GetPlayer(localNumber));
			InventoryBookMenu.RemovePlayer(networkNumber, player.UseController);
			Freeze();
			Disable();
			GameEventManager.SendEvent(new PlayerInGameRuleEvent(entered: false, networkNumber));
		}
		else
		{
			GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.NOITEMSELECTED));
		}
	}

	protected override void OnBack()
	{
		if (InventoryBookMenu.backEnabled || InventoryBookMenu.MainMenuBook)
		{
			if (InventoryBookMenu.backEnabled)
			{
				if (GameSettings.GetInstance().AvailableBlocks > 0)
				{
					if (!InventoryBookMenu.ScreenMode && InventoryBookMenu.currentPage != InventoryBookMenu.backPage)
					{
						InventoryBookMenu.GotoPage(InventoryBookMenu.backPage);
					}
					else
					{
						OnInventory();
					}
				}
				else if (InventoryBookMenu.ScreenMode && InventoryBookMenu.CurrentScreenpage == InventoryBookMenu.TabletPage)
				{
					GameEventManager.SendEvent(new SpecialUIEvent(SpecialUIEvent.SpecialUI.NOITEMSELECTED));
				}
			}
			else if (InventoryBookMenu.currentPage != 1)
			{
				InventoryBookMenu.GotoPage(1);
			}
			else
			{
				OnInventory();
			}
		}
		else
		{
			OnInventory();
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
