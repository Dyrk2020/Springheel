using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainMenuAnnouncement : MonoBehaviour
{
	public RawImage updatePic;

	public Animator animator;

	private bool shouldShow;

	private bool currentlyShown;

	private Texture2D loadedTexture;

	private bool updatingImage;

	private string lastImageFilename;

	private string lastAttemptedDownloadURL;

	private float timeWaited;

	protected Vector2 lastScreenSize;

	protected Camera UiCamera;

	public float horizontalPosition;

	public float minimumScale;

	protected Vector3 initialScale;

	protected Vector3 initialPosition;

	public Vector2 SafeAreaOffsetValues;

	protected float lastSafeAreaRatio;

	public Text titleText;

	public Text descriptionText;

	public static string locKey_LinkURL = "MainMenuUpdate/LinkURL";

	public static string locKey_ImageURL = "MainMenuUpdate/ImageURL";

	public static string locKey_Title = "MainMenuUpdate/Title";

	public static string locKey_Description = "MainMenuUpdate/Description";

	protected string url;

	protected string imageFilename;

	public static string MainMenuImageFolder => Application.persistentDataPath + "/mainmenu";

	private bool CanShow => loadedTexture != null;

	private string LatestImageURL => LocalizationManager.GetTranslation(locKey_ImageURL);

	private void Awake()
	{
		if (!Directory.Exists(MainMenuImageFolder))
		{
			Debug.Log("Creating main menu image folder at " + MainMenuImageFolder);
			Directory.CreateDirectory(MainMenuImageFolder);
		}
		UiCamera = GetComponentInParent<Camera>();
		initialScale = base.transform.localScale;
		initialPosition = base.transform.position;
		titleText.GetComponent<Localize>().Term = locKey_Title;
		descriptionText.GetComponent<Localize>().Term = locKey_Description;
	}

	private void OnDestroy()
	{
		if (loadedTexture != null)
		{
			UnityEngine.Object.Destroy(loadedTexture);
			loadedTexture = null;
		}
	}

	private void Update()
	{
		if (timeWaited < 3f)
		{
			timeWaited += Time.unscaledDeltaTime;
			return;
		}
		if ((float)Screen.width != lastScreenSize.x || (float)Screen.height != lastScreenSize.y || lastSafeAreaRatio != SafeAreaScaler.SafeAreaRatio)
		{
			lastSafeAreaRatio = SafeAreaScaler.SafeAreaRatio;
			Vector3 vector = UiCamera.ScreenToWorldPoint(new Vector3(UiCamera.pixelWidth, (float)UiCamera.pixelHeight * 0.9f, 250f));
			base.transform.position = new Vector3(horizontalPosition + vector.x + SafeAreaOffsetValues.x * SafeAreaScaler.SafeAreaRatioForLerp, initialPosition.y + SafeAreaOffsetValues.y * SafeAreaScaler.SafeAreaRatioForLerp, base.transform.position.z);
			float num = Mathf.Lerp(minimumScale, 1f, ((float)UiCamera.pixelWidth / (float)UiCamera.pixelHeight - 1.333f) / 0.444f);
			base.transform.localScale = initialScale * num;
			lastScreenSize.x = Screen.width;
			lastScreenSize.y = Screen.height;
		}
		if (!updatingImage && LatestImageURL != url)
		{
			url = LatestImageURL;
			imageFilename = GetLatestImageFilenameFromURL(url);
			if (imageFilename != lastImageFilename && imageFilename != null)
			{
				if (lastAttemptedDownloadURL == url)
				{
					return;
				}
				lastAttemptedDownloadURL = url;
				string imageFullPath = MainMenuImageFolder + "/" + imageFilename;
				UnityAction<byte[]> createImage = delegate(byte[] data)
				{
					if (data != null)
					{
						Texture2D texture2D = new Texture2D(1, 1);
						if (texture2D.LoadImage(data))
						{
							ReplaceLoadedTexture(texture2D);
							lastImageFilename = imageFilename;
						}
						else
						{
							Debug.LogError("Could not load texture from file.");
						}
					}
				};
				if (File.Exists(imageFullPath))
				{
					byte[] fileContents = null;
					WorkerThreadManager.Instance.AddFileOpJob(delegate
					{
						try
						{
							FileStream fileStream = File.OpenRead(imageFullPath);
							fileContents = new byte[fileStream.Length];
							fileStream.Read(fileContents, 0, (int)fileStream.Length);
							fileStream.Close();
						}
						catch (Exception ex)
						{
							Debug.LogError("Exception while reading main menu image: " + ex.Message + "\n" + ex.StackTrace);
						}
					}, delegate
					{
						createImage(fileContents);
					});
				}
				else
				{
					updatingImage = true;
					ReplaceLoadedTexture(null);
					StartCoroutine(DownloadUpdatePic());
				}
			}
		}
		if (loadedTexture == null && updatePic.enabled)
		{
			updatePic.enabled = false;
		}
		if (loadedTexture != null && !updatePic.enabled)
		{
			updatePic.enabled = true;
		}
		if (currentlyShown)
		{
			if (!shouldShow || !CanShow)
			{
				animator.SetBool("Show", value: false);
				currentlyShown = false;
			}
		}
		else if (shouldShow && CanShow)
		{
			animator.SetBool("Show", value: true);
			currentlyShown = true;
		}
	}

	private void ReplaceLoadedTexture(Texture2D texture)
	{
		if (!(this == null))
		{
			if (loadedTexture != null)
			{
				Debug.Log("Destroying old loaded texture for main menu update image");
				UnityEngine.Object.Destroy(loadedTexture);
			}
			loadedTexture = texture;
			updatePic.texture = texture;
			if (texture == null)
			{
				updatePic.enabled = false;
			}
		}
	}

	private string GetLatestImageFilenameFromURL(string url)
	{
		if (url.NullOrEmpty())
		{
			return null;
		}
		return Path.GetFileName(new Uri(url).LocalPath);
	}

	private IEnumerator DownloadUpdatePic()
	{
		string url = LatestImageURL;
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		www.SendWebRequest();
		float timeoutLimit = 15f;
		float t = 0f;
		while (!PlayerManager.GetInstance().FirstUserLoggedIn || !ControllerMonitor.Instance.IsMainControllerSet)
		{
			yield return null;
		}
		while (!www.isDone)
		{
			yield return null;
			t += Time.deltaTime;
			if (t > timeoutLimit)
			{
				Debug.LogError("Downloading update pic took more than " + timeoutLimit + " seconds...");
				updatingImage = false;
				yield break;
			}
		}
		if (www.error.NullOrEmpty())
		{
			if (IsImageContentType(www.GetResponseHeaders()))
			{
				string latestImageFilenameFromURL = GetLatestImageFilenameFromURL(url);
				string imageFullpath = MainMenuImageFolder + "/" + latestImageFilenameFromURL;
				if (File.Exists(imageFullpath))
				{
					File.Delete(imageFullpath);
				}
				Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
				if (texture != null)
				{
					byte[] bytes = www.downloadHandler.data;
					WorkerThreadManager.Instance.AddFileOpJob(delegate
					{
						QuickSaver.SaveBytesToFile(bytes, imageFullpath);
					});
					ReplaceLoadedTexture(texture);
					lastImageFilename = latestImageFilenameFromURL;
				}
				else
				{
					Debug.LogError("Could not load image from downloaded file");
				}
			}
			else
			{
				Debug.LogError("Downloaded data was not the right content type. URL may be incorrect.");
			}
		}
		else
		{
			Debug.LogError("WWW error: " + www.error);
		}
		updatingImage = false;
	}

	public void Show()
	{
		shouldShow = true;
	}

	public void Hide()
	{
		shouldShow = false;
	}

	private bool IsImageContentType(Dictionary<string, string> responseHeaders)
	{
		string value = null;
		if (responseHeaders.TryGetValue("CONTENT-TYPE", out value))
		{
			return value.StartsWith("image");
		}
		return false;
	}
}
