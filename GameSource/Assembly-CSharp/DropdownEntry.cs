using UnityEngine.UI;

public class DropdownEntry : PickableButton
{
	public DropdownMenu dropdown;

	public Text labelText;

	public int EntryValue;

	public object EntryData;

	protected override void Start()
	{
		base.Start();
		Enable();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		dropdown.OnClickDropdownEntry(this, triggerOnChangeEvent: true);
	}
}
