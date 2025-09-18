using UnityEngine;
using UnityEngine.Networking;

public class TreehouseButton : PickableButton
{
	private bool onHost;

	private float initialHoverScale;

	private bool returningToLobby;

	protected override void Awake()
	{
		base.Awake();
		initialHoverScale = hoveredScaleModifier;
	}

	protected override void Start()
	{
		base.Start();
		Enable();
	}

	public override void OnAccept(PickCursor pickCursor)
	{
		base.OnAccept(pickCursor);
		if (onHost && !returningToLobby)
		{
			returnToLobby();
		}
	}

	public void SetOnHost(bool onHost)
	{
		this.onHost = onHost;
		if (onHost)
		{
			SetAlpha(1f);
			hoveredScaleModifier = initialHoverScale;
		}
		else
		{
			SetAlpha(0f);
			hoveredScaleModifier = 0f;
			outlineHighlight.a = 0f;
		}
	}

	protected void returnToLobby()
	{
		returningToLobby = true;
		NetworkPlayerTracker playerTracker = LobbyManager.instance.PlayerTracker;
		if (playerTracker.WaitingForIDs)
		{
			Debug.LogWarning("Player tracker is still missing NetIDs");
		}
		foreach (uint allGameNetID in playerTracker.GetAllGameNetIDs())
		{
			if (allGameNetID == 0)
			{
				continue;
			}
			GameObject gameObject = ClientScene.FindLocalObject(new NetworkInstanceId(allGameNetID));
			if (!(gameObject == null))
			{
				GamePlayer component = gameObject.GetComponent<GamePlayer>();
				Character characterInstance = component.CharacterInstance;
				component.CharacterInstance = null;
				if (characterInstance != null)
				{
					characterInstance.transform.parent = null;
					Object.Destroy(characterInstance.gameObject);
				}
				Cursor cursorInstance = component.CursorInstance;
				component.CursorInstance = null;
				if (cursorInstance != null)
				{
					cursorInstance.transform.parent = null;
					Object.Destroy(cursorInstance.gameObject);
				}
			}
		}
		if (LobbyManager.instance.CurrentGameController != null)
		{
			LobbyManager.instance.CurrentGameController.EndGame();
		}
	}
}
