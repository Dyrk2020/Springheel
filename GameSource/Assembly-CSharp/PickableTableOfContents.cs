public class PickableTableOfContents : PickableButton
{
	public enum TableOfContentsJobs
	{
		GotoPage
	}

	public TableOfContentsJobs job;

	public InventoryPage.PageTypes targetPageType;

	public bool OnlineOnly;

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		if (job == TableOfContentsJobs.GotoPage)
		{
			inventoryBook.GotoPage(fakeVariable: false, targetPageType, enableBack: true);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!LobbyManager.instance.IsInOnlineGame && OnlineOnly && Visible)
		{
			Disable();
		}
	}
}
