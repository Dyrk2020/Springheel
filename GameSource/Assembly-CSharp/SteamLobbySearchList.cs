using System.Collections;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using UCHServices;
using UnityEngine;

public class SteamLobbySearchList : MonoBehaviour, InputReceiver
{
	public PickableNetworkButton SearchResultPrefab;

	public Transform[] ResultListPositions;

	public PickableNetworkButton RefreshButton;

	public PickableNetworkButton NextButton;

	public PickableNetworkButton BackButton;

	public PickableNetworkButton NumberPublicGamesFound;

	public InventoryBook inventoryBook;

	public InventoryPage inventoryPage;

	private List<PickableNetworkButton> listButtons = new List<PickableNetworkButton>();

	private List<string> searchResults = new List<string>();

	private Dictionary<string, Matchmaker.LobbyListInfo> FoundLobbies;

	private List<Matchmaker.LobbyListInfo> SortedLobbies = new List<Matchmaker.LobbyListInfo>();

	private int page;

	private bool searching;

	private IEnumerator coro;

	public int SortedLobbiesCount => SortedLobbies.Count;

	private int NumResultPages => (SortedLobbiesCount - 1) / ResultListPositions.Length + 1;

	private void Start()
	{
		Controller.AddGlobalReceiver(this);
		FoundLobbies = new Dictionary<string, Matchmaker.LobbyListInfo>();
	}

	private void OnDestroy()
	{
		Controller.RemoveGlobalReceiver(this);
	}

	public void ReceiveEvent(InputEvent e)
	{
		if (e.PlayerBitMask != 0 && (!(e.Sender is KeyboardInput) || Input.mouseScrollDelta.y == 0f))
		{
			if (e.Key == InputEvent.InputKey.RotateLeft && e.Valueb && e.Changed && PickableNetworkButton.currentResultPage > 0)
			{
				BackButton.OnAccept(null);
			}
			if (e.Key == InputEvent.InputKey.RotateRight && e.Valueb && e.Changed && PickableNetworkButton.currentResultPage < NumResultPages - 1)
			{
				NextButton.OnAccept(null);
			}
		}
	}

	private void Update()
	{
		if (PickableNetworkButton.currentResultPage != page)
		{
			page = PickableNetworkButton.currentResultPage;
			searching = true;
			coro = checkForListUpdates();
		}
		if (!searching && Matchmaker.Instance.Searching)
		{
			searching = true;
			coro = checkForListUpdates();
		}
		if (coro != null && !coro.MoveNext())
		{
			coro = null;
		}
	}

	public void AddFoundLobby(Matchmaker.LobbyListInfo lobby)
	{
		if (!FoundLobbies.ContainsKey(lobby.sLobbyID))
		{
			FoundLobbies.Add(lobby.sLobbyID, lobby);
		}
	}

	public void ClearList()
	{
		foreach (PickableNetworkButton listButton in listButtons)
		{
			Object.Destroy(listButton.gameObject);
		}
		listButtons.Clear();
		FoundLobbies.Clear();
		searchResults.Clear();
	}

	private IEnumerator checkForListUpdates()
	{
		foreach (PickableNetworkButton listButton in listButtons)
		{
			Object.Destroy(listButton.gameObject);
		}
		listButtons.Clear();
		searchResults.Clear();
		while (searching)
		{
			bool flag = true;
			List<Matchmaker.LobbyListInfo> list = new List<Matchmaker.LobbyListInfo>();
			List<Matchmaker.LobbyListInfo> list2 = new List<Matchmaker.LobbyListInfo>();
			List<Matchmaker.LobbyListInfo> list3 = new List<Matchmaker.LobbyListInfo>();
			foreach (Matchmaker.LobbyListInfo value in FoundLobbies.Values)
			{
				int regionFilterIndex = GameSettings.GetInstance().RegionFilterIndex;
				if (regionFilterIndex != -1)
				{
					AvailableRegion availableRegion = RelayConstants.AVAILABLE_REGIONS[regionFilterIndex];
					if (value.UnityServerRegion.id != availableRegion.id)
					{
						continue;
					}
				}
				if (value.matchProgress == 0)
				{
					if (value.Players == 4)
					{
						list3.Add(value);
					}
					else
					{
						list2.Add(value);
					}
				}
				else
				{
					list.Add(value);
				}
			}
			SortedLobbies.Clear();
			SortedLobbies = list2.OrderByDescending((Matchmaker.LobbyListInfo o) => o.CombinedHealthSkill).ToList();
			SortedLobbies.AddRange(list3);
			SortedLobbies.AddRange(list.OrderBy((Matchmaker.LobbyListInfo o) => (o.matchProgress != 0 || o.isAFK) ? o.matchProgress : (-1)).ToList());
			NumberPublicGamesFound.buttonText.text = "(" + ScriptLocalization.Network.Public_games_found + " " + SortedLobbies.Count + ")";
			for (int num = 0; num != SortedLobbies.Count; num++)
			{
				Matchmaker.LobbyListInfo lobbyListInfo = SortedLobbies[num];
				int num2 = num - page * ResultListPositions.Length;
				if (num2 >= 0)
				{
					if (num2 >= ResultListPositions.Length)
					{
						break;
					}
					if (!lobbyListInfo.InfoReceived)
					{
						flag = false;
					}
					else if (!searchResults.Contains(lobbyListInfo.sLobbyID))
					{
						searchResults.Add(lobbyListInfo.sLobbyID);
						PickableNetworkButton pickableNetworkButton = Object.Instantiate(SearchResultPrefab);
						pickableNetworkButton.SetSearchResultInfo(lobbyListInfo, num + 1);
						pickableNetworkButton.transform.SetParent(base.transform, worldPositionStays: false);
						pickableNetworkButton.pageNumber = inventoryPage.pageNumber;
						pickableNetworkButton.inventoryBook = inventoryBook;
						pickableNetworkButton.Enable();
						pickableNetworkButton.transform.position = ResultListPositions[num - page * ResultListPositions.Length].position;
						listButtons.Add(pickableNetworkButton);
					}
				}
			}
			if (SortedLobbies.Count > 0 && Matchmaker.Instance.LoadingSearchIndicator)
			{
				Matchmaker.Instance.LoadingSearchIndicator = false;
			}
			if (flag && !Matchmaker.Instance.Searching)
			{
				searching = false;
			}
			if (FoundLobbies.Values.Count == 0 && !GameState.OnlineDebugMode)
			{
				foreach (PickableNetworkButton listButton2 in listButtons)
				{
					Object.Destroy(listButton2.gameObject);
				}
				listButtons.Clear();
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private void addFakeInfo(List<Matchmaker.LobbyListInfo> lobbyList)
	{
		uint serverTime = Matchmaker.Instance.CurrentLobby.GetServerTime();
		Matchmaker.LobbyListInfo lobbyListInfo = new Matchmaker.LobbyListInfo();
		lobbyListInfo.LobbyOwner = "Chicken";
		lobbyListInfo.Players = 3;
		lobbyListInfo.UnityServerRegion = RelayConstants.AVAILABLE_REGIONS[0];
		lobbyListInfo.matchProgress = 0;
		lobbyListInfo.gameMode = GameState.GameMode.PARTY;
		lobbyListInfo.lastHearbeatTime = serverTime;
		lobbyListInfo.pointLimit = 250;
		lobbyListInfo.limitAmount = 12;
		lobbyListInfo.limitType = GameLimitType.ROUNDS;
		lobbyListInfo.LobbyHealthNum = 75;
		lobbyListInfo.InfoReceived = true;
		lobbyListInfo.ulLobbyID = 112343555452uL;
		lobbyList.Add(lobbyListInfo);
		lobbyListInfo.LobbyOwner = "Testing 123";
		lobbyListInfo.Players = 3;
		lobbyListInfo.UnityServerRegion = RelayConstants.AVAILABLE_REGIONS[0];
		lobbyListInfo.matchProgress = 0;
		lobbyListInfo.gameMode = GameState.GameMode.PARTY;
		lobbyListInfo.lastHearbeatTime = serverTime;
		lobbyListInfo.pointLimit = 1000;
		lobbyListInfo.limitAmount = 600;
		lobbyListInfo.limitType = GameLimitType.TIME;
		lobbyListInfo.LobbyHealthNum = 55;
		lobbyListInfo.InfoReceived = true;
		lobbyListInfo.ulLobbyID = 11123223443452uL;
		lobbyList.Add(lobbyListInfo);
		lobbyListInfo.LobbyOwner = "Mr Snooty Guy";
		lobbyListInfo.Players = 3;
		lobbyListInfo.UnityServerRegion = RelayConstants.AVAILABLE_REGIONS[0];
		lobbyListInfo.matchProgress = 50;
		lobbyListInfo.gameMode = GameState.GameMode.CREATIVE;
		lobbyListInfo.lastHearbeatTime = serverTime;
		lobbyListInfo.pointLimit = 150;
		lobbyListInfo.limitAmount = 20;
		lobbyListInfo.limitType = GameLimitType.ROUNDS;
		lobbyListInfo.LobbyHealthNum = 3;
		lobbyListInfo.InfoReceived = true;
		lobbyListInfo.ulLobbyID = 112334233343452uL;
		lobbyList.Add(lobbyListInfo);
		lobbyListInfo.LobbyOwner = "Testy Mc Test Face The Second Coming of the Test";
		lobbyListInfo.Players = 3;
		lobbyListInfo.UnityServerRegion = RelayConstants.AVAILABLE_REGIONS[0];
		lobbyListInfo.matchProgress = 25;
		lobbyListInfo.gameMode = GameState.GameMode.CREATIVE;
		lobbyListInfo.lastHearbeatTime = serverTime;
		lobbyListInfo.pointLimit = 800;
		lobbyListInfo.limitAmount = 300;
		lobbyListInfo.limitType = GameLimitType.TIME;
		lobbyListInfo.LobbyHealthNum = 65;
		lobbyListInfo.InfoReceived = true;
		lobbyListInfo.ulLobbyID = 112234232153443452uL;
		lobbyList.Add(lobbyListInfo);
		lobbyListInfo.LobbyOwner = "Testy Mc Test Face The Second Coming of the Test";
		lobbyListInfo.Players = 3;
		lobbyListInfo.UnityServerRegion = RelayConstants.AVAILABLE_REGIONS[0];
		lobbyListInfo.matchProgress = 80;
		lobbyListInfo.gameMode = GameState.GameMode.CREATIVE;
		lobbyListInfo.lastHearbeatTime = serverTime;
		lobbyListInfo.pointLimit = 800;
		lobbyListInfo.limitAmount = 300;
		lobbyListInfo.limitType = GameLimitType.TIME;
		lobbyListInfo.LobbyHealthNum = 90;
		lobbyListInfo.InfoReceived = true;
		lobbyListInfo.ulLobbyID = 1122323421423443452uL;
		lobbyList.Add(lobbyListInfo);
		lobbyListInfo.LobbyOwner = "Super Gamer Person";
		lobbyListInfo.Players = 3;
		lobbyListInfo.UnityServerRegion = RelayConstants.AVAILABLE_REGIONS[0];
		lobbyListInfo.matchProgress = 33;
		lobbyListInfo.gameMode = GameState.GameMode.CREATIVE;
		lobbyListInfo.lastHearbeatTime = serverTime;
		lobbyListInfo.pointLimit = 250;
		lobbyListInfo.limitAmount = 240;
		lobbyListInfo.limitType = GameLimitType.TIME;
		lobbyListInfo.LobbyHealthNum = 20;
		lobbyListInfo.InfoReceived = true;
		lobbyListInfo.ulLobbyID = 112234254343452uL;
		lobbyList.Add(lobbyListInfo);
	}
}
