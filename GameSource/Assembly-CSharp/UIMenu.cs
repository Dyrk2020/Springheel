using UnityEngine;

public class UIMenu : MonoBehaviour, InputReceiver
{
	public UIElement[] elements;

	public UINavigationMap navMap;

	public bool StartVisible;

	public int AssociatedPlayer;

	public UIMenu PreviousMenu;

	protected UIMenu nextMenu;

	public bool Visible { get; protected set; }

	protected virtual void Start()
	{
		elements = GetComponentsInChildren<UIElement>();
		Visible = StartVisible;
		if (!StartVisible)
		{
			Hide(useTransition: false);
		}
		Controller.AddGlobalReceiver(this);
	}

	private void Update()
	{
		if (nextMenu != null)
		{
			Hide();
			nextMenu.Show();
			nextMenu = null;
		}
	}

	public virtual void Show(bool useTransition = true)
	{
		Visible = true;
		if (navMap != null && navMap.currentNode.control != null)
		{
			navMap.currentNode.control.Select();
		}
		for (int i = 0; i != elements.Length; i++)
		{
			elements[i].Show();
		}
		UIMenu[] componentsInChildren = GetComponentsInChildren<UIMenu>();
		foreach (UIMenu uIMenu in componentsInChildren)
		{
			if (!(uIMenu == this) && !uIMenu.StartVisible)
			{
				uIMenu.Hide();
			}
		}
	}

	public virtual void Hide(bool useTransition = true)
	{
		Visible = false;
		for (int i = 0; i != elements.Length; i++)
		{
			elements[i].Hide();
		}
	}

	public virtual void ReceiveEvent(InputEvent e)
	{
		if (Visible && (AssociatedPlayer == 0 || (e.PlayerBitMask & (1 << AssociatedPlayer - 1)) != 0))
		{
			if (e.Key == InputEvent.InputKey.Back && e.Valueb && e.Changed)
			{
				GoToMenu(PreviousMenu);
			}
			else if (navMap != null)
			{
				navMap.HandleInputEvent(e);
			}
		}
	}

	public virtual void GoToMenu(UIMenu menu)
	{
		nextMenu = menu;
	}

	private void OnDestroy()
	{
		Controller.RemoveGlobalReceiver(this);
	}
}
