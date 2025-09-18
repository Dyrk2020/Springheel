public class Player
{
	public int TurnOrder;

	public GameControl.GamePhase FreeplayPhase;

	public bool HotseatPlayer;

	public bool Removed;

	public bool LoggedOut;

	private Character character;

	private Cursor cursor;

	private Controller controller;

	private LobbyPlayer assocLobbyPlayer;

	private GamePlayer assocGamePlayer;

	public bool Connected;

	public int Number { get; protected set; }

	public Character PlayerCharacter
	{
		get
		{
			return character;
		}
		set
		{
			if (character != null && controller != null)
			{
				character.PlayerNumber = 0;
				controller.RemoveReceiver(character);
				character.LocalPlayer = null;
			}
			character = value;
			if (character != null && controller != null)
			{
				character.PlayerNumber = Number;
				controller.AddReceiver(character);
				character.LocalPlayer = this;
			}
		}
	}

	public Cursor PlayerCursor
	{
		get
		{
			return cursor;
		}
		set
		{
			if (cursor != null && controller != null)
			{
				cursor.NetworklocalNumber = 0;
				controller.RemoveReceiver(cursor);
			}
			cursor = value;
			if (cursor != null && controller != null)
			{
				cursor.NetworklocalNumber = Number;
				controller.AddReceiver(cursor);
			}
		}
	}

	public Controller UseController
	{
		get
		{
			return controller;
		}
		set
		{
			if (controller != null)
			{
				if (PlayerCharacter != null)
				{
					controller.RemoveReceiver(PlayerCharacter);
				}
				if (PlayerCursor != null)
				{
					controller.RemoveReceiver(PlayerCursor);
				}
			}
			controller = value;
			if (controller != null)
			{
				controller.Connect(this);
				if (PlayerCharacter != null)
				{
					controller.AddReceiver(PlayerCharacter);
				}
				if (PlayerCursor != null)
				{
					controller.AddReceiver(PlayerCursor);
				}
			}
		}
	}

	public LobbyPlayer AssociatedLobbyPlayer
	{
		get
		{
			return assocLobbyPlayer;
		}
		set
		{
			if (assocLobbyPlayer != null && controller != null)
			{
				assocLobbyPlayer.LocalPlayer = null;
				controller.RemoveReceiver(assocLobbyPlayer);
			}
			assocLobbyPlayer = value;
			if (assocLobbyPlayer != null && controller != null)
			{
				assocLobbyPlayer.LocalPlayer = this;
				controller.AddReceiver(assocLobbyPlayer);
			}
		}
	}

	public GamePlayer AssociatedGamePlayer
	{
		get
		{
			return assocGamePlayer;
		}
		set
		{
			if (assocGamePlayer != null && controller != null)
			{
				assocGamePlayer.LocalPlayer = null;
			}
			assocGamePlayer = value;
			if (assocGamePlayer != null && controller != null)
			{
				assocGamePlayer.LocalPlayer = this;
			}
		}
	}

	public Player(int playerNumber)
	{
		Number = playerNumber;
	}

	public void Reset(bool full = true)
	{
		AssociatedGamePlayer = null;
		AssociatedLobbyPlayer = null;
		PlayerCharacter = null;
		PlayerCursor = null;
		Connected = false;
		HotseatPlayer = false;
		if (full)
		{
			if (UseController != null)
			{
				UseController.Reset();
			}
			UseController = null;
		}
	}
}
