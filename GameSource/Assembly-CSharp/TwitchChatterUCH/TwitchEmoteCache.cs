using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TwitchChatterUCH;

public class TwitchEmoteCache : MonoBehaviour
{
	public delegate void OnLoadCallback();

	[SerializeField]
	private Sprite _loadingIcon;

	[SerializeField]
	private Sprite _invalidIcon;

	private const string C_TWITCH_EMOTE_URL = "http://static-cdn.jtvnw.net/emoticons/v1/:emote_id/:size";

	private const string C_ID_REPLACE_PHRASE = ":emote_id";

	private const string C_SIZE_REPLACE_PHRASE = ":size";

	private static TwitchEmoteCache _singleton;

	private Dictionary<int, Sprite> _idSpriteMap;

	public static Sprite GetSpriteForEmoteID(int emoteID, OnLoadCallback callback = null)
	{
		if (_singleton != null)
		{
			if (_singleton._idSpriteMap.ContainsKey(emoteID))
			{
				return _singleton._idSpriteMap[emoteID];
			}
			_singleton.StartCoroutine(_singleton.LoadEmote(emoteID, callback));
			return _singleton._loadingIcon;
		}
		return null;
	}

	public static void Clear()
	{
		if (_singleton != null && _singleton._idSpriteMap != null)
		{
			int[] array = new int[_singleton._idSpriteMap.Keys.Count];
			_singleton._idSpriteMap.Keys.CopyTo(array, 0);
			int[] array2 = array;
			foreach (int id in array2)
			{
				_singleton.FreeMemoryForEmoteID(id);
			}
			_singleton._idSpriteMap.Clear();
		}
	}

	private void Awake()
	{
		_singleton = this;
		_idSpriteMap = new Dictionary<int, Sprite>();
	}

	private void OnDestroy()
	{
		Clear();
		_idSpriteMap = null;
		_singleton = null;
	}

	private IEnumerator LoadEmote(int emoteID, OnLoadCallback callback = null)
	{
		string url = "http://static-cdn.jtvnw.net/emoticons/v1/:emote_id/:size".Replace(":emote_id", string.Concat(emoteID)).Replace(":size", "1.0");
		WWW www = new WWW(url);
		yield return www;
		Sprite value;
		if (string.IsNullOrEmpty(www.error))
		{
			Texture2D texture2D = new Texture2D(32, 32, TextureFormat.ARGB32, mipChain: false);
			www.LoadImageIntoTexture(texture2D);
			value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		}
		else
		{
			value = _invalidIcon;
		}
		_idSpriteMap[emoteID] = value;
		callback?.Invoke();
	}

	private void FreeMemoryForEmoteID(int id)
	{
		if (_idSpriteMap.ContainsKey(id))
		{
			Sprite sprite = _idSpriteMap[id];
			if (sprite != _invalidIcon)
			{
				Object.Destroy(sprite.texture);
			}
			_idSpriteMap.Remove(id);
		}
	}
}
