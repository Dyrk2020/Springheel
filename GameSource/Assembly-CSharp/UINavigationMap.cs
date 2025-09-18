using UnityEngine;

public class UINavigationMap : MonoBehaviour
{
	public UINavigationNode startNode;

	public UIMenu menu;

	public UINavigationNode currentNode;

	private void Awake()
	{
		if (startNode != null)
		{
			startNode.control.Select();
			currentNode = startNode;
		}
	}

	private void Update()
	{
	}

	public bool HandleInputEvent(InputEvent e)
	{
		if (currentNode != null && !currentNode.HandleInputEvent(e))
		{
			if (e.Key == InputEvent.InputKey.OrthoUp && e.Valueb && e.Changed && currentNode.up != null)
			{
				if (currentNode.control != null)
				{
					currentNode.control.Deselect();
				}
				currentNode = currentNode.up;
				if (currentNode.control != null)
				{
					currentNode.control.Select();
				}
				return true;
			}
			if (e.Key == InputEvent.InputKey.OrthoDown && e.Valueb && e.Changed && currentNode.down != null)
			{
				if (currentNode.control != null)
				{
					currentNode.control.Deselect();
				}
				currentNode = currentNode.down;
				if (currentNode.control != null)
				{
					currentNode.control.Select();
				}
				return true;
			}
			if (e.Key == InputEvent.InputKey.OrthoLeft && e.Valueb && e.Changed && currentNode.left != null)
			{
				if (currentNode.control != null)
				{
					currentNode.control.Deselect();
				}
				currentNode = currentNode.left;
				if (currentNode.control != null)
				{
					currentNode.control.Select();
				}
				return true;
			}
			if (e.Key == InputEvent.InputKey.OrthoRight && e.Valueb && e.Changed && currentNode.right != null)
			{
				if (currentNode.control != null)
				{
					currentNode.control.Deselect();
				}
				currentNode = currentNode.right;
				if (currentNode.control != null)
				{
					currentNode.control.Select();
				}
				return true;
			}
			return false;
		}
		return true;
	}
}
