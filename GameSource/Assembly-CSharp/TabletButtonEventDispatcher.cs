using UnityEngine;

public class TabletButtonEventDispatcher : MonoBehaviour
{
	public TabletScreen tabletScreen;

	public TabletRule overlayType;

	public void OpenOverlay()
	{
		tabletScreen.OpenModalOverlay(overlayType);
	}
}
