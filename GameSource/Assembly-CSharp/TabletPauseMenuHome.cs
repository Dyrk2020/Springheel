using UnityEngine;

public class TabletPauseMenuHome : MonoBehaviour
{
	public TabletLoadedLevelScreen loadedLevelScreen;

	public TabletButton loadedLevelButton;

	public TabletButton saveShareButton;

	public TabletButton modifiersButton;

	private void Start()
	{
		bool flag = PlatformFeatureRestrictions.MustHideAllUGC || PlatformFeatureRestrictions.IsUGCRestricted;
		if (!loadedLevelScreen.CurrentlyInSnapshot || flag)
		{
			loadedLevelButton.SetDisabled(disabled: true);
		}
		switch (GameSettings.GetInstance().GameMode)
		{
		case GameState.GameMode.CHALLENGE:
			saveShareButton.SetDisabled(disabled: true);
			loadedLevelScreen.tablet.modifiersContainer.GetComponent<TabletDisableGroup>().SetDisabled(disabled: true);
			break;
		case GameState.GameMode.CREATIVE:
		case GameState.GameMode.PARTY:
			modifiersButton.gameObject.SetActive(value: false);
			break;
		}
		if (GameSettings.GetInstance().GameMode == GameState.GameMode.CHALLENGE)
		{
			saveShareButton.SetDisabled(disabled: true);
		}
		if (flag)
		{
			saveShareButton.SetDisabled(disabled: true);
		}
		if (flag)
		{
			loadedLevelButton.SetDisabled(disabled: true);
		}
	}
}
