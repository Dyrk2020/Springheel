using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneManagerWrapper
{
	public static bool IsInMainMenu => SceneManager.GetActiveScene().name == "MainMenu";

	public static void LoadScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	public static void LoadSceneAsync(string sceneName)
	{
		SceneManager.LoadSceneAsync(sceneName);
	}

	public static void UnloadSceneAsync(string sceneName)
	{
		SceneManager.UnloadSceneAsync(sceneName);
	}

	public static IEnumerator DoGentleSceneLoad(string sceneName)
	{
		bool loadingTreehouse = sceneName == "TreeHouseLobby";
		yield return new WaitForSeconds(0.5f);
		Scene currentScene = SceneManager.GetActiveScene();
		GameObject[] rootGameObjects = currentScene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			UnityEngine.Object.Destroy(rootGameObjects[i]);
		}
		yield return null;
		AsyncOperation asyncUnloadSceneOp = SceneManager.UnloadSceneAsync(currentScene);
		while (asyncUnloadSceneOp != null && !asyncUnloadSceneOp.isDone)
		{
			yield return null;
		}
		Debug.Log("Unloading unused assets");
		AsyncOperation unloadAssets = Resources.UnloadUnusedAssets();
		while (!unloadAssets.isDone)
		{
			yield return null;
		}
		Debug.Log("Collecting Garbage");
		GC.Collect();
		yield return null;
		Debug.Log("Actually loading " + sceneName);
		if (loadingTreehouse)
		{
			LobbyManagerManager.BeforeLoadingTreehouse();
		}
		AsyncOperation asyncLoadSceneOp = SceneManager.LoadSceneAsync(sceneName);
		while (asyncLoadSceneOp != null && !asyncLoadSceneOp.isDone)
		{
			yield return null;
		}
	}
}
