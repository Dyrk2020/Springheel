using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class BeamParticleAssigner : NetworkBehaviour, IGameEventListener
{
	public BeamParticles BeamParticlePrefab;

	public Transform ParticleTarget;

	private bool setup;

	private static int kRpcRpcSpawnBeams;

	private void Start()
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding: true);
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding: false);
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (setup)
		{
			return;
		}
		if (base.hasAuthority)
		{
			GameControl gameControl = Object.FindObjectOfType<GameControl>();
			if (gameControl != null)
			{
				foreach (GamePlayer item in gameControl.CurrentPlayerQueue)
				{
					CallRpcSpawnBeams(item.CharacterInstance.gameObject);
				}
			}
		}
		setup = true;
	}

	[ClientRpc]
	private void RpcSpawnBeams(GameObject character)
	{
		BeamParticles beamParticles = Object.Instantiate(BeamParticlePrefab, character.transform);
		beamParticles.transform.localPosition = Vector3.up;
		beamParticles.SetTarget(ParticleTarget);
		beamParticles.SetCharacter(character.GetComponent<Character>());
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcSpawnBeams(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSpawnBeams called on server.");
		}
		else
		{
			((BeamParticleAssigner)obj).RpcSpawnBeams(reader.ReadGameObject());
		}
	}

	public void CallRpcSpawnBeams(GameObject character)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSpawnBeams called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSpawnBeams);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(character);
		SendRPCInternal(networkWriter, 0, "RpcSpawnBeams");
	}

	static BeamParticleAssigner()
	{
		kRpcRpcSpawnBeams = 596868048;
		NetworkBehaviour.RegisterRpcDelegate(typeof(BeamParticleAssigner), kRpcRpcSpawnBeams, InvokeRpcRpcSpawnBeams);
		NetworkCRC.RegisterBehaviour("BeamParticleAssigner", 0);
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
