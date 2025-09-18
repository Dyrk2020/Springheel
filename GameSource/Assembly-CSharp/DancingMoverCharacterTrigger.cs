using System.Collections.Generic;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class DancingMoverCharacterTrigger : MonoBehaviour, IGameEventListener
{
	public DancingMovingMultipiecePlatform dancingMovingPlatform;

	private HashSet<int> localCharactersDancing = new HashSet<int>();

	private HashSet<int> localCharactersOnPlatform = new HashSet<int>();

	private HashSet<int> activeDancerNetworkNumbers = new HashSet<int>();

	private HashSet<int> activeOnPlatformNetworkNumbers = new HashSet<int>();

	private Dictionary<int, float> lastValidDancingTimePerCharacter = new Dictionary<int, float>();

	private Dictionary<int, float> lastValidOnPlatformTimePerCharacter = new Dictionary<int, float>();

	private List<int> inactiveNetworkNumbersToRemove = new List<int>();

	private Dictionary<Character, Collider2D> cachedFeetColliders = new Dictionary<Character, Collider2D>();

	private BoxCollider2D characterTriggerCollider;

	private const float gracePeriod = 0.1f;

	private void Awake()
	{
		characterTriggerCollider = GetComponent<BoxCollider2D>();
	}

	private void OnEnable()
	{
		ChangeListener(adding: true);
	}

	private void OnDisable()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
	}

	public void handleEvent(global::GameEvent.GameEvent gameEvent)
	{
		if (!(gameEvent.GetType() == typeof(NetworkMessageReceivedEvent)))
		{
			return;
		}
		NetworkMessageReceivedEvent networkMessageReceivedEvent = gameEvent as NetworkMessageReceivedEvent;
		if (networkMessageReceivedEvent.Message.msgType == NetMsgTypes.PlatformDancing)
		{
			if (NetworkServer.active)
			{
				ProcessServerMessage(networkMessageReceivedEvent.ReadMessage as MsgPlatformDancing);
			}
			else
			{
				ProcessClientMessage(networkMessageReceivedEvent.ReadMessage as MsgPlatformDancing);
			}
		}
	}

	private void ProcessServerMessage(MsgPlatformDancing message)
	{
		if (message == null || message.PlatformID != dancingMovingPlatform.ID)
		{
			return;
		}
		bool flag = false;
		if (message.IsDancing)
		{
			if (activeDancerNetworkNumbers.Add(message.PlayerNumber))
			{
				flag = true;
			}
		}
		else if (activeDancerNetworkNumbers.Remove(message.PlayerNumber))
		{
			flag = true;
		}
		if (message.CharacterOnPlatform)
		{
			if (activeOnPlatformNetworkNumbers.Add(message.PlayerNumber))
			{
				flag = true;
			}
		}
		else if (activeOnPlatformNetworkNumbers.Remove(message.PlayerNumber))
		{
			flag = true;
		}
		if (flag)
		{
			UpdatePlatformStateAndBroadcast();
		}
	}

	private void ProcessClientMessage(MsgPlatformDancing message)
	{
		if (message != null && message.PlatformID == dancingMovingPlatform.ID)
		{
			dancingMovingPlatform.DancingCharacter = message.IsDancing;
			dancingMovingPlatform.CharacterOnPlatform = message.CharacterOnPlatform;
		}
	}

	private void Update()
	{
		CheckLocalCharactersState();
		CleanUpDisconnectedCharacters();
	}

	private void CheckLocalCharactersState()
	{
		foreach (Character allCharacter in Character.AllCharacters)
		{
			if (allCharacter != null && allCharacter.hasAuthority)
			{
				ProcessLocalCharacterState(allCharacter);
			}
		}
	}

	private void ProcessLocalCharacterState(Character character)
	{
		int networkNumber = character.networkNumber;
		bool num = IsCharacterPhysicallyOnPlatform(character);
		bool flag = character.CurrentAnim == Character.AnimState.WIN;
		bool flag2 = num;
		bool num2 = num && flag;
		if (flag2)
		{
			lastValidOnPlatformTimePerCharacter[networkNumber] = Time.time;
		}
		if (num2)
		{
			lastValidDancingTimePerCharacter[networkNumber] = Time.time;
		}
		float value = 0f;
		lastValidOnPlatformTimePerCharacter.TryGetValue(networkNumber, out value);
		float value2 = 0f;
		lastValidDancingTimePerCharacter.TryGetValue(networkNumber, out value2);
		bool flag3 = Time.time - value <= 0.1f;
		bool flag4 = Time.time - value2 <= 0.1f;
		bool flag5 = localCharactersOnPlatform.Contains(networkNumber);
		bool flag6 = localCharactersDancing.Contains(networkNumber);
		if (flag3 != flag5 || flag4 != flag6)
		{
			if (flag3)
			{
				localCharactersOnPlatform.Add(networkNumber);
			}
			else
			{
				localCharactersOnPlatform.Remove(networkNumber);
			}
			if (flag4)
			{
				localCharactersDancing.Add(networkNumber);
			}
			else
			{
				localCharactersDancing.Remove(networkNumber);
			}
			SendPlatformDancingMessage(networkNumber, flag4, flag3);
		}
	}

	private void CleanUpDisconnectedCharacters()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		inactiveNetworkNumbersToRemove.Clear();
		foreach (int activeOnPlatformNetworkNumber in activeOnPlatformNetworkNumbers)
		{
			bool flag = false;
			foreach (Character allCharacter in Character.AllCharacters)
			{
				if (allCharacter != null && allCharacter.networkNumber == activeOnPlatformNetworkNumber)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				inactiveNetworkNumbersToRemove.Add(activeOnPlatformNetworkNumber);
			}
		}
		bool flag2 = false;
		foreach (int item in inactiveNetworkNumbersToRemove)
		{
			if (activeDancerNetworkNumbers.Remove(item))
			{
				flag2 = true;
			}
			if (activeOnPlatformNetworkNumbers.Remove(item))
			{
				flag2 = true;
			}
		}
		if (flag2)
		{
			UpdatePlatformStateAndBroadcast();
		}
	}

	private void UpdatePlatformStateAndBroadcast()
	{
		bool flag = activeDancerNetworkNumbers.Count > 0;
		bool characterOnPlatform = activeOnPlatformNetworkNumbers.Count > 0;
		dancingMovingPlatform.DancingCharacter = flag;
		dancingMovingPlatform.CharacterOnPlatform = characterOnPlatform;
		MsgPlatformDancing msgPlatformDancing = new MsgPlatformDancing();
		msgPlatformDancing.PlatformID = dancingMovingPlatform.ID;
		msgPlatformDancing.IsDancing = flag;
		msgPlatformDancing.CharacterOnPlatform = characterOnPlatform;
		msgPlatformDancing.PlatformPosition = dancingMovingPlatform.transform.position;
		NetworkServer.SendToAll(NetMsgTypes.PlatformDancing, msgPlatformDancing);
	}

	private bool IsCharacterPhysicallyOnPlatform(Character character)
	{
		if (character.Dying || (character.Dead && !character.isGhost))
		{
			return false;
		}
		bool num = character.IsStandingOn(dancingMovingPlatform.gameObject);
		bool flag = IsCharacterInsideTriggerBounds(character);
		return num || flag;
	}

	private bool IsCharacterInsideTriggerBounds(Character character)
	{
		if (characterTriggerCollider == null)
		{
			return false;
		}
		Bounds bounds = characterTriggerCollider.bounds;
		Collider2D feetCollider = GetFeetCollider(character);
		if (feetCollider != null && feetCollider.enabled && CheckBounds2D(bounds, feetCollider.bounds))
		{
			return true;
		}
		if (character.LowerBodyTrigger != null && character.LowerBodyTrigger.enabled && CheckBounds2D(bounds, character.LowerBodyTrigger.bounds))
		{
			return true;
		}
		return false;
	}

	private Collider2D GetFeetCollider(Character character)
	{
		if (character.FeetPhysicsCollider == null)
		{
			return null;
		}
		if (!cachedFeetColliders.TryGetValue(character, out var value) || value == null)
		{
			value = character.FeetPhysicsCollider.GetComponent<Collider2D>();
			cachedFeetColliders[character] = value;
		}
		return value;
	}

	private bool CheckBounds2D(Bounds firstBounds, Bounds secondBounds)
	{
		if (firstBounds.min.x <= secondBounds.max.x && firstBounds.max.x >= secondBounds.min.x && firstBounds.min.y <= secondBounds.max.y)
		{
			return firstBounds.max.y >= secondBounds.min.y;
		}
		return false;
	}

	private void SendPlatformDancingMessage(int playerNetworkNumber, bool isDancing, bool isCharacterOnPlatform)
	{
		MsgPlatformDancing msgPlatformDancing = new MsgPlatformDancing();
		msgPlatformDancing.PlatformID = dancingMovingPlatform.ID;
		msgPlatformDancing.PlayerNumber = playerNetworkNumber;
		msgPlatformDancing.IsDancing = isDancing;
		msgPlatformDancing.CharacterOnPlatform = isCharacterOnPlatform;
		msgPlatformDancing.PlatformPosition = dancingMovingPlatform.transform.position;
		if (NetworkManager.singleton != null && NetworkManager.singleton.client != null)
		{
			NetworkManager.singleton.client.Send(NetMsgTypes.PlatformDancing, msgPlatformDancing);
		}
	}
}
