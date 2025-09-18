using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TabletSidebar : MonoBehaviour
{
	private enum AnimState
	{
		Open,
		Opening,
		Closed,
		Closing
	}

	public TabletColorScheme colorScheme;

	public Transform sidebarContainer;

	public TabletSubdialogController subdialogController;

	public RectTransform mainmenuHomePage;

	public RectTransform treehouseHomePage;

	public RectTransform pauseHomePage;

	public List<RectTransform> hideForClients;

	private AnimState currentState = AnimState.Closed;

	private float animTime;

	private Vector3 startPos;

	private Vector3 endPos;

	public AnimationCurve openCurve;

	public AnimationCurve closeCurve;

	public float openCloseTime = 0.2f;

	public bool IsOpen
	{
		get
		{
			AnimState animState = currentState;
			if ((uint)animState <= 1u)
			{
				return true;
			}
			return false;
		}
	}

	private void Awake()
	{
		startPos = sidebarContainer.GetComponent<RectTransform>().anchoredPosition;
		endPos = Vector3.zero;
		if (!(LobbyManager.instance != null) || LobbyManager.instance.IsHost)
		{
			return;
		}
		foreach (RectTransform hideForClient in hideForClients)
		{
			hideForClient.gameObject.SetActive(value: false);
		}
	}

	private void Start()
	{
		if (LobbyManager.instance == null || SceneManager.GetActiveScene().name == "MainMenu")
		{
			subdialogController.ForceSubdialog(mainmenuHomePage);
		}
		else if (LobbyManager.instance.CurrentLevelSelectController != null)
		{
			subdialogController.ForceSubdialog(treehouseHomePage);
		}
		else
		{
			subdialogController.ForceSubdialog(pauseHomePage);
		}
	}

	private void Update()
	{
		switch (currentState)
		{
		case AnimState.Opening:
		{
			animTime += Time.deltaTime;
			if (animTime >= openCloseTime)
			{
				animTime = openCloseTime;
				currentState = AnimState.Open;
			}
			float alpha = openCurve.Evaluate(animTime / openCloseTime);
			UpdateSidebarPosition(alpha);
			break;
		}
		case AnimState.Closing:
		{
			animTime -= Time.deltaTime;
			if (animTime <= 0f)
			{
				animTime = 0f;
				currentState = AnimState.Closed;
			}
			float alpha = closeCurve.Evaluate(animTime / openCloseTime);
			UpdateSidebarPosition(alpha);
			break;
		}
		}
	}

	private void UpdateSidebarPosition(float alpha)
	{
		sidebarContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.LerpUnclamped(startPos, endPos, alpha);
	}

	public void Open()
	{
		AnimState animState = currentState;
		if ((uint)(animState - 2) <= 1u)
		{
			currentState = AnimState.Opening;
		}
	}

	public void Close()
	{
		AnimState animState = currentState;
		if ((uint)animState <= 1u)
		{
			currentState = AnimState.Closing;
		}
	}

	public void ResetStyles()
	{
		TabletButton[] componentsInChildren = GetComponentsInChildren<TabletButton>(includeInactive: true);
		foreach (TabletButton obj in componentsInChildren)
		{
			obj.colorScheme = colorScheme;
			obj.ResetStyles();
		}
	}

	public void OnClickResumeGame(PickCursor pickCursor)
	{
		if (!LobbyManager.instance.IsInOnlineGame)
		{
			GameState.GetInstance().Paused = false;
			GameEventManager.SendEvent(new PauseEvent(pause: false, pickCursor.networkNumber));
		}
		else
		{
			GameEventManager.SendEvent(new SoftPauseEvent(softpause: false, pickCursor.networkNumber, hostPausing: false));
		}
	}
}
