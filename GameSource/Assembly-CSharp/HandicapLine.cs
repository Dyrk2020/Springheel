using GameEvent;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HandicapLine : MonoBehaviour, IGameEventListener
{
	public Text AnimalName;

	public Text HandicapNumber;

	public GameObject ScorelineStretcher;

	protected Vector3 initialScale;

	public int PlayerNetworkNumber;

	public float animateSpeed = 0.4f;

	private Character.Animals currentAnimal;

	private bool isWearingSkin;

	private int targetHandicap = -999;

	private float currentHandicapFloat = 1f;

	private bool currentlyShown = true;

	private void Start()
	{
		initialScale = ScorelineStretcher.transform.localScale;
		ChangeListener(adding: true);
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	private void Update()
	{
		bool flag = false;
		if (LobbyManager.instance != null && LobbyManager.instance.CurrentLevelSelectController != null)
		{
			NetworkLobbyPlayer[] lobbySlots = LobbyManager.instance.lobbySlots;
			for (int i = 0; i < lobbySlots.Length; i++)
			{
				LobbyPlayer lobbyPlayer = (LobbyPlayer)lobbySlots[i];
				if (!(lobbyPlayer == null) && lobbyPlayer.networkNumber == PlayerNetworkNumber && lobbyPlayer.PickedAnimal != Character.Animals.NONE)
				{
					if (lobbyPlayer.handicap != targetHandicap)
					{
						targetHandicap = lobbyPlayer.handicap;
						HandicapNumber.text = targetHandicap + "%";
					}
					bool flag2 = lobbyPlayer.IsWearingSkin;
					if (currentAnimal != lobbyPlayer.PickedAnimal || isWearingSkin != flag2)
					{
						currentAnimal = lobbyPlayer.PickedAnimal;
						isWearingSkin = flag2;
						SetName(currentAnimal, isWearingSkin);
					}
					flag = true;
				}
			}
		}
		if (flag != currentlyShown)
		{
			currentlyShown = flag;
			Show(flag);
		}
		if (flag)
		{
			currentHandicapFloat = Mathf.MoveTowards(currentHandicapFloat, (float)targetHandicap / 100f, animateSpeed * Time.unscaledDeltaTime);
			ScorelineStretcher.transform.localScale = new Vector3(initialScale.x * currentHandicapFloat, initialScale.y, initialScale.y);
		}
	}

	public void SetName(Character.Animals animal, bool altSkin)
	{
		AnimalName.text = Character.GetLocalizedAnimal(animal, altSkin);
	}

	public void Show(bool show)
	{
		ScorelineStretcher.SetActive(show);
		AnimalName.gameObject.SetActive(show);
		HandicapNumber.gameObject.SetActive(show);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<LanguageChangeEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(LanguageChangeEvent))
		{
			LobbyPlayer lobbyPlayer = LobbyManager.instance.GetLobbyPlayer(PlayerNetworkNumber);
			SetName(currentAnimal, lobbyPlayer != null && lobbyPlayer.IsWearingSkin);
		}
	}
}
