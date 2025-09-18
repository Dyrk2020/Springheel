using UnityEngine.Events;

public class GenericButton : PickableButton
{
	public UnityEvent OnClick;

	public GenericButtonEvent OnClickWithCursor;

	protected override void Start()
	{
		base.Start();
		Enable();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		if (OnClick != null)
		{
			OnClick.Invoke();
		}
		if (OnClickWithCursor != null)
		{
			OnClickWithCursor.Invoke(pickCursor);
		}
	}
}
