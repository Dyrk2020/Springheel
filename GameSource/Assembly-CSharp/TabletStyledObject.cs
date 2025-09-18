using UnityEngine;

public class TabletStyledObject : MonoBehaviour
{
	public TabletColorScheme colorScheme;

	[HideInInspector]
	[SerializeField]
	protected bool disabled;

	[SerializeField]
	[HideInInspector]
	protected bool interactable = true;

	public bool Disabled => disabled;

	public bool Interactable => interactable;

	public virtual bool TracksCursors => false;

	public virtual void ResetStyles()
	{
		if (colorScheme == null)
		{
			colorScheme = GetComponentInParent<TabletColorScheme>();
			if (colorScheme != null)
			{
				Debug.LogWarning("Automatically gave " + base.name + " color scheme from " + colorScheme.name);
			}
			else
			{
				Debug.LogError("Could not find a parent color scheme for " + base.name);
			}
		}
	}

	public virtual void SetDisabled(bool disabled)
	{
		this.disabled = disabled;
	}

	public virtual void SetInteractable(bool interactable)
	{
		this.interactable = interactable;
	}

	public virtual void AddTrackedCursor(PickCursor pickCursor)
	{
	}

	public virtual void RemoveTrackedCursor(PickCursor pickCursor)
	{
	}
}
