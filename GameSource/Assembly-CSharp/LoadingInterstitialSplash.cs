using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class LoadingInterstitialSplash : CanvasSplash
{
	private static LoadingInterstitialSplash instance;

	public Text levelName;

	public Text levelCode;

	public UGCNameTag authorNameTag;

	public Text CurrentGameMode;

	private Canvas canvas;

	private bool fadingVolume;

	public bool showLevelInfoNextLoad;

	public static LoadingInterstitialSplash Instance => instance;

	protected override void Awake()
	{
		base.Awake();
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		ClearWordFilteredLabels();
		canvas = GetComponent<Canvas>();
	}

	protected override void Start()
	{
		base.Start();
		if (instance == this)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	private void ClearWordFilteredLabels()
	{
		if (authorNameTag != null)
		{
			authorNameTag.usernameText.text = "";
		}
		levelName.text = "";
	}

	public override void FadeIn()
	{
		fadingVolume = true;
		if (canvas != null)
		{
			canvas.enabled = true;
		}
		base.FadeIn();
		if (showLevelInfoNextLoad)
		{
			LevelSelectController.PlayedSnapshotInfo currentSnapshotInfo = GameState.GetInstance().currentSnapshotInfo;
			CurrentGameMode.gameObject.SetActive(value: true);
			levelName.gameObject.SetActive(value: true);
			if (!currentSnapshotInfo.snapshotName.NullOrEmpty())
			{
				levelName.text = currentSnapshotInfo.snapshotName;
				if (!currentSnapshotInfo.authorID.NullOrEmpty())
				{
					authorNameTag.gameObject.SetActive(value: true);
					authorNameTag.InitializeAsync(currentSnapshotInfo.authorDisplayName, currentSnapshotInfo.authorPlatformID, currentSnapshotInfo.authorID, currentSnapshotInfo.authorPlatform);
				}
				else
				{
					authorNameTag.gameObject.SetActive(value: false);
				}
				if (currentSnapshotInfo.snapshotCode.NullOrEmpty())
				{
					levelCode.gameObject.SetActive(value: false);
					levelCode.text = "";
				}
				else
				{
					levelCode.gameObject.SetActive(value: true);
					levelCode.text = GameSparksQuery.GetFormattedSnapshotCode(currentSnapshotInfo.snapshotCode);
				}
			}
			else
			{
				levelCode.gameObject.SetActive(value: false);
				authorNameTag.gameObject.SetActive(value: false);
				levelName.text = LevelSelectController.GetLocalizedLevelName(currentSnapshotInfo.nextLevel);
			}
			GameState.GameMode gameMode = GameSettings.GetInstance().GameMode;
			CurrentGameMode.text = ScriptLocalization.InLobby.Mode + ": ";
			switch (gameMode)
			{
			case GameState.GameMode.PARTY:
				CurrentGameMode.text += ScriptLocalization.InLobby.PartyText;
				break;
			case GameState.GameMode.CREATIVE:
				CurrentGameMode.text += ScriptLocalization.InLobby.CreativeText;
				break;
			case GameState.GameMode.CHALLENGE:
				CurrentGameMode.text += ScriptLocalization.InLobby.ChallengeModeButtonText;
				break;
			case GameState.GameMode.FREEPLAY:
				CurrentGameMode.text += ScriptLocalization.InLobby.FreePlayButtonText;
				break;
			}
			showLevelInfoNextLoad = false;
		}
		else
		{
			CurrentGameMode.gameObject.SetActive(value: false);
			levelCode.gameObject.SetActive(value: false);
			levelName.gameObject.SetActive(value: false);
			authorNameTag.gameObject.SetActive(value: false);
		}
	}

	public override void FadeOut()
	{
		base.FadeOut();
		fadingVolume = true;
	}

	public override void Show()
	{
		base.Show();
		if (canvas != null)
		{
			canvas.enabled = true;
		}
	}

	public override void Hide()
	{
		base.Hide();
		if (canvas != null)
		{
			canvas.enabled = false;
		}
		ClearWordFilteredLabels();
	}
}
