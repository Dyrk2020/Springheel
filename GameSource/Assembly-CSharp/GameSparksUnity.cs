using GameSparks.Platforms;
using UnityEngine;

public class GameSparksUnity : MonoBehaviour
{
	public GameSparksSettings settings;

	private void Start()
	{
		base.gameObject.AddComponent<DefaultPlatform>();
	}
}
