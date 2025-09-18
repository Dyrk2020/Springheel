using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class StatsSaveGame : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public bool UseSecondarySave;

	public bool Save;

	public void OnPointerClick(PointerEventData eventData)
	{
		bool useSecondarySaveFile = GameSettings.GetInstance().useSecondarySaveFile;
		GameSettings.GetInstance().useSecondarySaveFile = UseSecondarySave;
		if (Save)
		{
			StatTracker.Instance.SaveGameForAllUsers();
		}
		else
		{
			Load();
		}
		GameSettings.GetInstance().useSecondarySaveFile = useSecondarySaveFile;
	}

	private void Load()
	{
		string text = Application.dataPath + "/";
		if (Application.isEditor && GameSettings.GetInstance().IgnoreSaveFileInEditor)
		{
			return;
		}
		string text2 = (GameSettings.GetInstance().useSecondarySaveFile ? "saveData-Beta.uch" : "saveData.uch");
		if (File.Exists(text + text2))
		{
			try
			{
				StreamReader streamReader = File.OpenText(text + text2);
				string s = streamReader.ReadToEnd();
				streamReader.Close();
				byte[] bytes = Convert.FromBase64String(s);
				string text3 = Encoding.UTF8.GetString(bytes);
				Debug.Log(text3);
				SaveFileData saveFileData = StatTracker.Instance.CreateSaveFileDataForMainUser();
				XMLSaver.Load(text3, saveFileData);
			}
			catch (Exception ex)
			{
				Debug.LogError("Could not load save file: (" + ex.GetType().ToString() + ") " + ex.Message + ".\n" + ex.StackTrace);
			}
		}
	}
}
