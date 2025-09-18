using UnityEngine;
using UnityEngine.UI;

public class TabletGamertagButton : MonoBehaviour
{
	public RawImage rawImage;

	public Text buttonText;

	public Image border;

	public void OnClickXboxGamertag(PickCursor pickCursor)
	{
	}

	protected void Show(bool show)
	{
		if (buttonText != null)
		{
			buttonText.enabled = show;
		}
		if (border != null)
		{
			border.enabled = show;
		}
		if (rawImage != null)
		{
			rawImage.enabled = show;
		}
	}
}
