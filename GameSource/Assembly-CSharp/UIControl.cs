public class UIControl : UIElement
{
	public bool Selected { get; protected set; }

	private void Start()
	{
	}

	private void Update()
	{
	}

	public override void Show()
	{
	}

	public override void Hide(bool forceQuickHide = false)
	{
	}

	public virtual void Enable()
	{
	}

	public virtual void Disable()
	{
	}

	public virtual void Select()
	{
		Selected = true;
	}

	public virtual void Deselect()
	{
		Selected = false;
	}

	public virtual bool HandleInputEvent(InputEvent e)
	{
		return false;
	}
}
