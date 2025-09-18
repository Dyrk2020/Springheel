using UnityEngine;

public class TabletPresetEntry : MonoBehaviour
{
	public TabletTextLabel nameLabel;

	public TabletButton deleteButton;

	private int presetIdx;

	public void Initialize(int presetIdx, string presetName, bool showDelete)
	{
		this.presetIdx = presetIdx;
		nameLabel.text = presetName;
		deleteButton.gameObject.SetActive(showDelete);
	}

	public void OnClickDeleteButton(PickCursor pickCursor)
	{
		GetComponentInParent<TabletPresetSelectOverlay>().OnClickDeletePreset(presetIdx);
	}

	public void OnClickPresetEntry(PickCursor pickCursor)
	{
		GetComponentInParent<TabletPresetSelectOverlay>().OnSelectPreset(presetIdx);
	}
}
