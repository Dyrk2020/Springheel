using System;
using System.Collections;
using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class PartyPickCursor : PickCursor, IGameEventListener
{
	[SyncVar]
	public bool SpawnedOnClient;

	private static int kCmdCmdSetSpawned;

	public bool NetworkSpawnedOnClient
	{
		get
		{
			return SpawnedOnClient;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref SpawnedOnClient, 8u);
		}
	}

	public override void Start()
	{
		base.Start();
		GameEventManager.SendEvent(new PartyCursorSpawnedEvent(this));
		if (base.hasAuthority)
		{
			CallCmdSetSpawned(isSpawned: true);
		}
	}

	protected override void OnRotateLeft()
	{
	}

	protected override void OnRotateRight()
	{
	}

	protected override void OnBack()
	{
	}

	protected override void OnInventory()
	{
	}

	protected override void OnAccept()
	{
		if (hoverStatePick() && lastHoveredPick != null && lastHoveredPick.GetType() == typeof(PickableBlock))
		{
			StartCoroutine(tryPickPiece());
		}
		if (hoverStatePick() && lastHoveredPick != null && lastHoveredPick.GetType() == typeof(PickableButton))
		{
			lastHoveredPick.OnAccept(this);
		}
	}

	protected override IEnumerator tryPickPiece()
	{
		while (PickCursor.placementLock)
		{
			yield return null;
		}
		PickCursor.placementLock = true;
		lockSet = true;
		if (pickedPiece == null && hoveredPick != null)
		{
			Picking = false;
			AkSoundEngine.PostEvent("UI_Inventory_Select_" + hoveredPick.SFXEventName, base.gameObject);
			AkSoundEngine.PostEvent("UI_InGame_PartyBox_Select_Item", base.gameObject);
			base.NetworkpickedPiece = ClientScene.FindLocalObject(new NetworkInstanceId(hoveredPick.netIdValue));
			if (base.hasAuthority)
			{
				MsgPiecePicked msgPiecePicked = new MsgPiecePicked();
				msgPiecePicked.PickableNetID = hoveredPick.netIdValue;
				msgPiecePicked.PlayerNumber = networkNumber;
				NetworkManager.singleton.client.Send(NetMsgTypes.PiecePicked, msgPiecePicked);
			}
			Freeze();
			Disable();
		}
	}

	public override void ChangeListener(bool adding)
	{
		base.ChangeListener(adding);
		GameEventManager.ChangeListener<NetworkMessageReceivedEvent>(this, adding);
		GameEventManager.ChangeListener<PlacementTimerDoneEvent>(this, adding);
	}

	public override void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				base.Networkpaused = true;
				Pause();
			}
			else
			{
				base.Networkpaused = false;
				Unpause();
			}
		}
		else
		{
			base.handleEvent(e);
		}
		if (type == typeof(SoftPauseEvent))
		{
			SoftPauseEvent softPauseEvent = e as SoftPauseEvent;
			if (softPauseEvent.SoftPaused && softPauseEvent.PlayerNumber == networkNumber)
			{
				base.Networkpaused = true;
				Pause();
			}
			else if (!softPauseEvent.SoftPaused && softPauseEvent.PlayerNumber == networkNumber)
			{
				base.Networkpaused = false;
				Unpause();
			}
		}
		if (type == typeof(PlacementTimerDoneEvent))
		{
			Debug.Log("Received PlacementTimerDoneEvent");
			if (base.Enabled && base.hasAuthority)
			{
				MsgPiecePicked msgPiecePicked = new MsgPiecePicked();
				msgPiecePicked.PickableNetID = 0u;
				msgPiecePicked.PlayerNumber = networkNumber;
				NetworkManager.singleton.client.Send(NetMsgTypes.PiecePicked, msgPiecePicked);
			}
			Freeze();
			Disable();
		}
	}

	[Command]
	private void CmdSetSpawned(bool isSpawned)
	{
		NetworkSpawnedOnClient = isSpawned;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetSpawned(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetSpawned called on client.");
		}
		else
		{
			((PartyPickCursor)obj).CmdSetSpawned(reader.ReadBoolean());
		}
	}

	public void CallCmdSetSpawned(bool isSpawned)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetSpawned called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetSpawned(isSpawned);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetSpawned);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(isSpawned);
		SendCommandInternal(networkWriter, 0, "CmdSetSpawned");
	}

	static PartyPickCursor()
	{
		kCmdCmdSetSpawned = -1030875915;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PartyPickCursor), kCmdCmdSetSpawned, InvokeCmdCmdSetSpawned);
		NetworkCRC.RegisterBehaviour("PartyPickCursor", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(SpawnedOnClient);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(SpawnedOnClient);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			SpawnedOnClient = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 8) != 0)
		{
			SpawnedOnClient = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
