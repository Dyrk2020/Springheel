using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class CursorControlHintButton : MonoBehaviour
{
	public CursorControlHints.Button button;

	public Text buttonText;

	private bool visibleCache = true;

	private bool highlightCache;

	private string stringCache = "";

	public Image image;

	private Color DefaultImageColor;

	private Color DefaultTextColor;

	public Color highlightImageColor;

	public Color highlightTextColor;

	private void Awake()
	{
		image = GetComponentInChildren<Image>();
		if (image != null)
		{
			DefaultImageColor = image.color;
		}
		if (buttonText != null)
		{
			DefaultTextColor = buttonText.color;
		}
	}

	public void SetVisible(bool visible, string textKey = null, bool highlighted = false)
	{
		bool flag = textKey != null && stringCache.CompareTo(textKey) != 0;
		if (!(visibleCache != visible || highlighted != highlightCache || flag))
		{
			return;
		}
		visibleCache = visible;
		base.gameObject.SetActive(visible);
		if (highlighted != highlightCache)
		{
			highlightCache = highlighted;
			if (highlighted)
			{
				image.color = highlightImageColor;
				buttonText.color = highlightTextColor;
			}
			else
			{
				image.color = DefaultImageColor;
				buttonText.color = DefaultTextColor;
			}
		}
		if (flag)
		{
			stringCache = textKey;
			buttonText.text = LocalizationManager.GetTranslation(textKey);
		}
	}
}
