using UnityEngine;
using UnityEngine.UI;

namespace TwitchChatterUCH;

public class TwitchEmoteImage : Image
{
	private int _emoteID = -1;

	private float _scaleFactor = 1f;

	public int emoteID
	{
		get
		{
			return _emoteID;
		}
		set
		{
			_emoteID = value;
			RefreshImage();
		}
	}

	public float scaleFactor
	{
		get
		{
			return _scaleFactor;
		}
		set
		{
			if (_scaleFactor != value)
			{
				_scaleFactor = value;
				RefreshImage();
			}
		}
	}

	private void onLoadCallback()
	{
		RefreshImage();
	}

	private void RefreshImage()
	{
		if (_emoteID > 0)
		{
			base.sprite = TwitchEmoteCache.GetSpriteForEmoteID(_emoteID, onLoadCallback);
			RectTransform obj = base.rectTransform;
			float num = _scaleFactor;
			float width = base.sprite.rect.width;
			obj.sizeDelta = num * new Vector2(width, base.sprite.rect.height);
		}
	}
}
