public class LockItem : ActiveBlock
{
	public override void Enable()
	{
		base.Enable();
		LockItem[] componentsInChildren = GetComponentsInChildren<LockItem>();
		foreach (LockItem lockItem in componentsInChildren)
		{
			if (!(lockItem == this))
			{
				lockItem.Enable();
			}
		}
	}

	public override void Disable()
	{
		base.Disable();
		LockItem[] componentsInChildren = GetComponentsInChildren<LockItem>();
		foreach (LockItem lockItem in componentsInChildren)
		{
			if (!(lockItem == this))
			{
				lockItem.Disable();
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		Enable();
	}
}
