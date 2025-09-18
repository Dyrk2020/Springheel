using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crosstales.BWF.Data;
using Crosstales.BWF.Model;
using Crosstales.BWF.Util;
using Crosstales.Common.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace Crosstales.BWF.Provider;

[HelpURL("https://www.crosstales.com/media/data/assets/badwordfilter/api/class_crosstales_1_1_b_w_f_1_1_provider_1_1_bad_word_provider_text.html")]
public class BadWordProviderText : BadWordProvider
{
	private bool _webSuccess;

	public override void Load()
	{
		base.Load();
		if (Sources == null)
		{
			return;
		}
		_loading = true;
		if (BaseHelper.isEditorMode)
		{
			return;
		}
		foreach (Source item in Sources.Where((Source source) => source != null))
		{
			_webSuccess = false;
			if (!string.IsNullOrEmpty(item.URL))
			{
				StartCoroutine(loadWeb(item));
			}
			if (item.Resource != null && (!item.IsResourceFallback || (item.IsResourceFallback && !_webSuccess)))
			{
				StartCoroutine(loadResource(item));
			}
		}
	}

	public override void Save()
	{
		Debug.LogWarning("Save not implemented!", this);
	}

	private IEnumerator loadWeb(Source src)
	{
		string uid = Guid.NewGuid().ToString();
		coRoutines.Add(uid);
		if (NetworkHelper.isURL(src.URL))
		{
			using UnityWebRequest www = UnityWebRequest.Get(src.URL.Trim());
			www.timeout = Constants.WWW_TIMEOUT;
			www.downloadHandler = new DownloadHandlerBuffer();
			yield return www.SendWebRequest();
			if (www.result != UnityWebRequest.Result.ProtocolError && www.result != UnityWebRequest.Result.ConnectionError)
			{
				List<string> list = BaseHelper.SplitStringToLines(www.downloadHandler.text);
				yield return null;
				if (Config.DEBUG)
				{
					Debug.Log($"{src}: {list.Count}", this);
				}
				src.Regexes = list.ToArray();
				if (list.Count > 0)
				{
					_badwords.Add(new BadWords(src, list));
					_webSuccess = true;
				}
				else
				{
					Debug.LogWarning("Source '" + src.URL + "' does not contain any active bad words!", this);
				}
			}
			else
			{
				Debug.LogWarning("Could not load source '" + src.URL + "'" + Environment.NewLine + www.error + Environment.NewLine + "Did you set the correct 'URL'?", this);
			}
		}
		else
		{
			Debug.LogWarning("'URL' is invalid, null or empty!" + Environment.NewLine + "Please add a valid URL.", this);
		}
		coRoutines.Remove(uid);
		if (_loading && coRoutines.Count == 0)
		{
			_loading = false;
			init();
		}
	}

	private IEnumerator loadResource(Source src)
	{
		string uid = Guid.NewGuid().ToString();
		coRoutines.Add(uid);
		if (src.Resource != null)
		{
			List<string> list = BaseHelper.SplitStringToLines(src.Resource.text);
			yield return null;
			if (Config.DEBUG)
			{
				Debug.Log($"{src}: {list.Count}", this);
			}
			src.Regexes = list.ToArray();
			if (list.Count > 0)
			{
				_badwords.Add(new BadWords(src, list));
			}
			else
			{
				Debug.LogWarning($"Resource '{src.Resource}' does not contain any active bad words!", this);
			}
		}
		else
		{
			Debug.LogWarning("Resource field 'Source' is null or empty!" + Environment.NewLine + "Please add a valid resource.", this);
		}
		coRoutines.Remove(uid);
		if (_loading && coRoutines.Count == 0)
		{
			_loading = false;
			init();
		}
	}
}
