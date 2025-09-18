using UnityEngine;

public abstract class UIElement : MonoBehaviour
{
	public UIMenu menu;

	public abstract void Show();

	public abstract void Hide(bool forceQuickhide = false);
}
