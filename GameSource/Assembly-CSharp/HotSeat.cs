using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotSeat : MonoBehaviour
{
	protected class Seat
	{
		public Vector3 position;

		public bool occupied;

		public Character character;

		public Seat(Vector3 pos)
		{
			position = pos;
		}
	}

	public Transform[] SeatPositions;

	protected Seat[] seats;

	protected Dictionary<Controller, Player[]> playerControlMap = new Dictionary<Controller, Player[]>();

	protected List<Character> charactersAtCouch = new List<Character>();

	private bool hidden;

	private void Start()
	{
		GameState instance = GameState.GetInstance();
		playerControlMap.Add(instance.Keyboard, new Player[4]);
		seats = new Seat[SeatPositions.Length];
		for (int i = 0; i != seats.Length; i++)
		{
			seats[i] = new Seat(SeatPositions[i].position);
		}
	}

	private void Update()
	{
		if (LobbyManager.instance != null)
		{
			if (!LobbyManager.instance.IsInOnlineGame)
			{
				show();
			}
			else
			{
				hide();
			}
		}
	}

	private void hide()
	{
		if (!hidden)
		{
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			Text[] componentsInChildren2 = GetComponentsInChildren<Text>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = false;
			}
			hidden = true;
		}
	}

	private void show()
	{
		if (hidden)
		{
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
			Text[] componentsInChildren2 = GetComponentsInChildren<Text>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = true;
			}
			hidden = false;
		}
	}

	public void SitPlayer(Player player)
	{
		if (hidden)
		{
			return;
		}
		Seat[] array = seats;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].character == player.PlayerCharacter)
			{
				return;
			}
		}
		array = seats;
		foreach (Seat seat in array)
		{
			if (seat.occupied)
			{
				continue;
			}
			seat.occupied = true;
			seat.character = player.PlayerCharacter;
			seat.character.transform.position = seat.position;
			seat.character.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			seat.character.Ready = true;
			seat.character.Sitting = true;
			SpriteRenderer[] componentsInChildren = seat.character.GetComponentsInChildren<SpriteRenderer>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].sortingLayerName = "Default2";
			}
			if (playerControlMap.ContainsKey(player.UseController))
			{
				Player[] array2 = playerControlMap[player.UseController];
				for (int k = 0; k != 4; k++)
				{
					if (array2[k] == null)
					{
						array2[k] = player;
						break;
					}
				}
			}
			else
			{
				playerControlMap.Add(player.UseController, new Player[4]);
				playerControlMap[player.UseController][0] = player;
			}
			AkSoundEngine.PostEvent("UI_Lobby_Shared_Couch_Selected", player.PlayerCharacter.gameObject);
			break;
		}
	}

	public void UnsitPlayer(Player player)
	{
		Seat[] array = seats;
		foreach (Seat seat in array)
		{
			if (seat.character == player.PlayerCharacter)
			{
				seat.occupied = false;
				seat.character.Ready = false;
				seat.character.Sitting = false;
				SpriteRenderer[] componentsInChildren = seat.character.GetComponentsInChildren<SpriteRenderer>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].sortingLayerName = "Player";
				}
				seat.character = null;
				AkSoundEngine.PostEvent("UI_Lobby_Shared_Couch_Deelected", player.PlayerCharacter.gameObject);
			}
		}
		Player[] array2 = playerControlMap[player.UseController];
		for (int k = 0; k != 4; k++)
		{
			if (array2[k] == player)
			{
				array2[k] = null;
			}
		}
	}

	public bool IsSeatAvailable()
	{
		if (hidden)
		{
			return false;
		}
		Seat[] array = seats;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].occupied)
			{
				return true;
			}
		}
		return false;
	}

	public int GetSeatsTaken()
	{
		if (seats == null)
		{
			return 0;
		}
		int num = 0;
		Seat[] array = seats;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].occupied)
			{
				num++;
			}
		}
		return num;
	}

	public bool PlayerSitting(Player player)
	{
		if (!playerControlMap.ContainsKey(player.UseController))
		{
			return false;
		}
		Player[] array = playerControlMap[player.UseController];
		for (int i = 0; i != 4; i++)
		{
			if (array[i] == player)
			{
				return true;
			}
		}
		return false;
	}

	public int PlayersWithController(Controller c)
	{
		if (playerControlMap.ContainsKey(c))
		{
			int num = 0;
			Player[] array = playerControlMap[c];
			for (int i = 0; i != 4; i++)
			{
				if (array[i] != null)
				{
					num++;
				}
			}
			return num;
		}
		return 0;
	}

	public Player[] GetAllPlayersWithController(Controller c)
	{
		if (!playerControlMap.ContainsKey(c))
		{
			return new Player[0];
		}
		int num = 0;
		Player[] array = playerControlMap[c];
		Player[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i] != null)
			{
				num++;
			}
		}
		Player[] array3 = new Player[num];
		num = 0;
		array2 = array;
		foreach (Player player in array2)
		{
			if (player != null)
			{
				array3[num] = player;
				num++;
			}
		}
		return array3;
	}

	public Player GetLastPlayerWithController(Controller c)
	{
		if (!playerControlMap.ContainsKey(c))
		{
			return null;
		}
		Player[] array = playerControlMap[c];
		for (int num = 3; num >= 0; num--)
		{
			if (array[num] != null && array[num].UseController == c)
			{
				return array[num];
			}
		}
		return null;
	}

	public bool CharacterAtCouch(Character c)
	{
		return charactersAtCouch.Contains(c);
	}

	private void OnTriggerEnter2D(Collider2D c)
	{
		Character componentInParent = c.gameObject.GetComponentInParent<Character>();
		if (componentInParent != null && !charactersAtCouch.Contains(componentInParent))
		{
			charactersAtCouch.Add(componentInParent);
		}
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		Character componentInParent = c.gameObject.GetComponentInParent<Character>();
		if (componentInParent != null && charactersAtCouch.Contains(componentInParent))
		{
			charactersAtCouch.Remove(componentInParent);
		}
	}
}
