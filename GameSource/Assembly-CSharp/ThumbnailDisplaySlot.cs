using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ThumbnailDisplaySlot : MonoBehaviour
{
	public SpriteRenderer thumbnailSpinner;

	public RawImage thumbnailImage;

	public Text thumbnailNotFoundText;

	private string waitingForHash;

	public void HideImage()
	{
		thumbnailImage.enabled = false;
	}

	public void LoadThumbnail(string code, string levelName)
	{
		string text = null;
		if (!code.NullOrEmpty())
		{
			text = GameSparksQuery.SanitizeSnapshotCode(code);
		}
		thumbnailSpinner.enabled = true;
		thumbnailNotFoundText.enabled = false;
		thumbnailSpinner.enabled = true;
		if (text.NullOrEmpty())
		{
			LevelThumbnailCache.Instance.LoadLocalSaveThumbnail(levelName, GetOnTextureFound(levelName));
		}
		else
		{
			LevelThumbnailCache.Instance.LoadThumbnailFromCloud(text, GetOnTextureFound(code + levelName));
		}
	}

	private UnityAction<Texture2D> GetOnTextureFound(string hash)
	{
		waitingForHash = hash;
		return delegate(Texture2D tex)
		{
			if (!(hash != waitingForHash))
			{
				if (tex != null)
				{
					thumbnailNotFoundText.enabled = false;
					thumbnailImage.enabled = true;
					thumbnailImage.texture = tex;
				}
				else
				{
					thumbnailNotFoundText.enabled = true;
					thumbnailImage.enabled = false;
					thumbnailImage.texture = null;
				}
				thumbnailSpinner.enabled = false;
			}
		};
	}
}
