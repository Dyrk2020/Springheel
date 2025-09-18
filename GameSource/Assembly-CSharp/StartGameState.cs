using Cysharp.Threading.Tasks;
using I2.Loc;
using UnityEngine;

public class StartGameState : MonoBehaviour
{
	private int waitingFor;

	public bool LoadMainMenu;

	private void Awake()
	{
		if (!GameSettings.GetInstance().DebugOutsideEditor && !Debug.isDebugBuild)
		{
			Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
			Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
		}
		UpdateLocalizationAsync();
		Debug.Log("Waiting to initialize: " + waitingFor + (LoadMainMenu ? " then loading main menu" : ""));
		LobbyManagerManager.Instance.Initialize();
		WorkerThreadManager.Instance.Initialize();
		LevelThumbnailCache.Instance.Initialize();
		Transporter.Instance.Initialize();
		Placeable.attachmentColliderMask = LayerMask.GetMask("Placed", "Fixed");
	}

	private async UniTaskVoid UpdateLocalizationAsync()
	{
		await UniTask.WaitUntil(() => LocalizationManager.Sources.Count > 0);
		Debug.Log($"Handling language stuff : {LocalizationManager.Sources.Count}");
		foreach (LanguageSourceData source in LocalizationManager.Sources)
		{
			source.GoogleUpdateFrequency = LanguageSourceData.eGoogleUpdateFrequency.Always;
			source.Import_Google(ForceUpdate: true, justCheck: false);
			source.UpdateDictionary();
		}
	}

	private void Update()
	{
		if (waitingFor <= 0 && LoadMainMenu)
		{
			Debug.Log("Done waiting for initialization, loading main menu.");
			SceneManagerWrapper.LoadScene("MainMenu");
		}
	}
}
