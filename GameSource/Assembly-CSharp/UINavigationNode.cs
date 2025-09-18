using UnityEngine;

public class UINavigationNode : MonoBehaviour
{
	public UIControl control;

	public UINavigationNode up;

	public UINavigationNode down;

	public UINavigationNode left;

	public UINavigationNode right;

	private void Awake()
	{
		if (control != null)
		{
			control.Deselect();
		}
	}

	private void Update()
	{
	}

	public bool HandleInputEvent(InputEvent e)
	{
		if (control != null)
		{
			return control.HandleInputEvent(e);
		}
		return false;
	}
}
