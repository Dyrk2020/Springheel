using UnityEngine;

public class TabletKeyboardConfig : MonoBehaviour
{
	public void OnClickReset(PickCursor pickCursor)
	{
		StatTracker.Instance.ClearKeybindings();
		TabletKeyboardBindingButton[] componentsInChildren = GetComponentsInChildren<TabletKeyboardBindingButton>();
		foreach (TabletKeyboardBindingButton obj in componentsInChildren)
		{
			obj.CancelRebind();
			obj.RefreshBinding();
		}
	}

	public void CancelCurrentBinding()
	{
		TabletKeyboardBindingButton[] componentsInChildren = GetComponentsInChildren<TabletKeyboardBindingButton>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].CancelRebind();
		}
	}
}
