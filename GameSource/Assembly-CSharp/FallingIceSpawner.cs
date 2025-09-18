using System;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class FallingIceSpawner : NetworkBehaviour, IGameEventListener
{
	public FallingIce[] fallingIcePrefabs;

	public Transform startPoint;

	public Transform EndPoint;

	public float averagePeriod;

	public float randomRange;

	public float shooterTimer;

	public float maxRotateSpeed;

	public float minScale;

	public float maxScale;

	private static int kCmdCmdSpawnIceBlock;

	private static int kRpcRpcSpawnIceBlock;

	public bool Paused { get; protected set; }

	public bool scoreboard { get; protected set; }

	public void Start()
	{
		ChangeListener(adding: true);
	}

	public void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<ScoreboardEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
	}

	protected void FixedUpdate()
	{
		if (!Paused && !scoreboard && base.hasAuthority)
		{
			shooterTimer -= Time.fixedDeltaTime;
			if (shooterTimer < 0f)
			{
				CallCmdSpawnIceBlock();
				shooterTimer = averagePeriod + UnityEngine.Random.Range(0f - randomRange, randomRange);
			}
		}
	}

	[Command]
	private void CmdSpawnIceBlock()
	{
		CallRpcSpawnIceBlock(UnityEngine.Random.Range(0, fallingIcePrefabs.Length), Vector3.Lerp(startPoint.position, EndPoint.position, UnityEngine.Random.Range(0f, 1f)), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0f - maxRotateSpeed, maxRotateSpeed), UnityEngine.Random.Range(minScale, maxScale));
	}

	[ClientRpc(channel = 0)]
	private void RpcSpawnIceBlock(int iceType, Vector3 position, float rotation, float rotateSpeed, float Scale)
	{
		FallingIce fallingIce = UnityEngine.Object.Instantiate(fallingIcePrefabs[iceType], position, Quaternion.Euler(0f, 0f, rotation));
		fallingIce.GetComponent<Rigidbody2D>().angularVelocity = rotateSpeed;
		fallingIce.transform.localScale = Vector3.one * Scale;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				Paused = true;
			}
			else
			{
				Paused = false;
			}
		}
		if (type == typeof(ScoreboardEvent))
		{
			if ((e as ScoreboardEvent).Showing)
			{
				scoreboard = true;
			}
			else
			{
				scoreboard = false;
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSpawnIceBlock(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnIceBlock called on client.");
		}
		else
		{
			((FallingIceSpawner)obj).CmdSpawnIceBlock();
		}
	}

	public void CallCmdSpawnIceBlock()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnIceBlock called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnIceBlock();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnIceBlock);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdSpawnIceBlock");
	}

	protected static void InvokeRpcRpcSpawnIceBlock(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpawnIceBlock called on server.");
		}
		else
		{
			((FallingIceSpawner)obj).RpcSpawnIceBlock((int)reader.ReadPackedUInt32(), reader.ReadVector3(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}
	}

	public void CallRpcSpawnIceBlock(int iceType, Vector3 position, float rotation, float rotateSpeed, float Scale)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSpawnIceBlock called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSpawnIceBlock);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)iceType);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(rotateSpeed);
		networkWriter.Write(Scale);
		SendRPCInternal(networkWriter, 0, "RpcSpawnIceBlock");
	}

	static FallingIceSpawner()
	{
		kCmdCmdSpawnIceBlock = 2080681049;
		NetworkBehaviour.RegisterCommandDelegate(typeof(FallingIceSpawner), kCmdCmdSpawnIceBlock, InvokeCmdCmdSpawnIceBlock);
		kRpcRpcSpawnIceBlock = 372110403;
		NetworkBehaviour.RegisterRpcDelegate(typeof(FallingIceSpawner), kRpcRpcSpawnIceBlock, InvokeRpcRpcSpawnIceBlock);
		NetworkCRC.RegisterBehaviour("FallingIceSpawner", 0);
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
