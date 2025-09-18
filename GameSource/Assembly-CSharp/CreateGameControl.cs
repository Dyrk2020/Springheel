using UnityEngine;
using UnityEngine.Networking;

public class CreateGameControl : NetworkBehaviour
{
	public GameControl[] GameModePrefabs;

	public Level LevelLayout;

	public Graphpaper GraphPaper;

	private void Awake()
	{
		if (!NetworkServer.active && !GetComponent<NetworkIdentity>().isServer)
		{
			return;
		}
		if (LevelLayout == null)
		{
			Debug.LogError("Missing level layout reference");
		}
		if (GraphPaper == null)
		{
			Debug.LogError("Missing graph paper reference");
		}
		int gameMode = (int)GameSettings.GetInstance().GameMode;
		if (gameMode < GameModePrefabs.Length)
		{
			GameControl gameControl = GameModePrefabs[gameMode];
			if (gameControl != null)
			{
				GameControl gameControl2 = Object.Instantiate(gameControl);
				gameControl2.NetworkAssociatedScene = base.gameObject.scene.name;
				NetworkServer.Spawn(gameControl2.gameObject);
			}
			else
			{
				Debug.LogError("Missing game controller prefab for game mode: " + GameSettings.GetInstance().GameMode);
			}
		}
		else
		{
			Debug.LogError("Missing game controller prefab for game mode: " + GameSettings.GetInstance().GameMode);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
