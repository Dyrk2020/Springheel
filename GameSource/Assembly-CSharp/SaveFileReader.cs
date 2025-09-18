using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

public class SaveFileReader : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(waitForAllStart());
	}

	private void LoadSave()
	{
		string text = Application.persistentDataPath + "/";
		if ((Application.isEditor && GameSettings.GetInstance().IgnoreSaveFileInEditor) || !File.Exists(text + (GameSettings.GetInstance().useSecondarySaveFile ? "saveData-Beta.uch" : "saveData.uch")))
		{
			return;
		}
		try
		{
			StreamReader streamReader = File.OpenText(text + (GameSettings.GetInstance().useSecondarySaveFile ? "saveData-Beta.uch" : "saveData.uch"));
			string s = streamReader.ReadToEnd();
			streamReader.Close();
			byte[] bytes = Convert.FromBase64String(s);
			string xml = Encoding.UTF8.GetString(bytes);
			SaveFileData saveFileData = StatTracker.Instance.CreateSaveFileDataForMainUser();
			XMLSaver.Load(xml, saveFileData);
			StatReader[] array = UnityEngine.Object.FindObjectsOfType<StatReader>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Reload();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not load save file: (" + ex.GetType().ToString() + ") " + ex.Message + ".\n" + ex.StackTrace);
		}
	}

	private IEnumerator waitForAllStart()
	{
		yield return new WaitForEndOfFrame();
		LoadSave();
	}

	private void Update()
	{
	}
}
