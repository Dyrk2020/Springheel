using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreInitialization : MonoBehaviour
{
	private async void Start()
	{
		await InitializationFlow();
	}

	private async UniTask InitializationFlow()
	{
		await SceneManager.LoadSceneAsync("Init");
	}
}
