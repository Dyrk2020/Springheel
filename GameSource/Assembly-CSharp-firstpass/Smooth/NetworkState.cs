using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Smooth;

public class NetworkState : MessageBase
{
	public enum SyncInfo
	{
		NONE,
		POSITION,
		ROTATION,
		BOTH
	}

	public SmoothSync smoothSync;

	public State state = new State();

	private byte positionMask = 1;

	private byte rotationMask = 2;

	private byte velocityMask = 4;

	private byte angularVelocityMask = 8;

	public NetworkState()
	{
	}

	public NetworkState(SmoothSync smoothSyncScript)
	{
		smoothSync = smoothSyncScript;
		state = new State(smoothSyncScript);
	}

	public override void Serialize(NetworkWriter writer)
	{
		bool flag = smoothSync.shouldSendPosition();
		bool flag2 = smoothSync.shouldSendVelocity();
		bool flag3 = smoothSync.shouldSendAngularVelocity();
		bool flag4 = smoothSync.shouldSendRotation();
		if (!NetworkServer.active)
		{
			if (flag)
			{
				smoothSync.lastPositionWhenStateWasSent = state.position;
			}
			if (flag2)
			{
				smoothSync.lastVelocityWhenStateWasSent = state.velocity;
			}
			if (flag3)
			{
				smoothSync.lastAngularVelocityWhenStateWasSent = state.angularVelocity;
			}
			if (flag4)
			{
				smoothSync.lastRotationWhenStateWasSent = state.rotation;
			}
		}
		writer.Write(encodeSyncInformation(flag, flag4, flag2, flag3));
		writer.Write(smoothSync.netId);
		writer.WritePackedUInt32((uint)smoothSync.syncIndex);
		writer.WritePackedUInt32((uint)state.ownerTimestamp);
		if (flag)
		{
			if (smoothSync.isPositionCompressed)
			{
				if (smoothSync.isSyncingXPosition)
				{
					writer.Write(HalfHelper.Compress(state.position.x));
				}
				if (smoothSync.isSyncingYPosition)
				{
					writer.Write(HalfHelper.Compress(state.position.y));
				}
				if (smoothSync.isSyncingZPosition)
				{
					writer.Write(HalfHelper.Compress(state.position.z));
				}
			}
			else
			{
				if (smoothSync.isSyncingXPosition)
				{
					writer.Write(state.position.x);
				}
				if (smoothSync.isSyncingYPosition)
				{
					writer.Write(state.position.y);
				}
				if (smoothSync.isSyncingZPosition)
				{
					writer.Write(state.position.z);
				}
			}
		}
		if (flag2)
		{
			if (smoothSync.isVelocityCompressed)
			{
				if (smoothSync.isSyncingXVelocity)
				{
					writer.Write(HalfHelper.Compress(state.velocity.x));
				}
				if (smoothSync.isSyncingYVelocity)
				{
					writer.Write(HalfHelper.Compress(state.velocity.y));
				}
				if (smoothSync.isSyncingZVelocity)
				{
					writer.Write(HalfHelper.Compress(state.velocity.z));
				}
			}
			else
			{
				if (smoothSync.isSyncingXVelocity)
				{
					writer.Write(state.velocity.x);
				}
				if (smoothSync.isSyncingYVelocity)
				{
					writer.Write(state.velocity.y);
				}
				if (smoothSync.isSyncingZVelocity)
				{
					writer.Write(state.velocity.z);
				}
			}
		}
		if (flag4)
		{
			Vector3 eulerAngles = state.rotation.eulerAngles;
			if (smoothSync.isRotationCompressed)
			{
				if (smoothSync.isSyncingXRotation)
				{
					writer.Write(HalfHelper.Compress(eulerAngles.x));
				}
				if (smoothSync.isSyncingYRotation)
				{
					writer.Write(HalfHelper.Compress(eulerAngles.y));
				}
				if (smoothSync.isSyncingZRotation)
				{
					writer.Write(HalfHelper.Compress(eulerAngles.z));
				}
			}
			else
			{
				if (smoothSync.isSyncingXRotation)
				{
					writer.Write(eulerAngles.x);
				}
				if (smoothSync.isSyncingYRotation)
				{
					writer.Write(eulerAngles.y);
				}
				if (smoothSync.isSyncingZRotation)
				{
					writer.Write(eulerAngles.z);
				}
			}
		}
		if (!flag3)
		{
			return;
		}
		if (smoothSync.isAngularVelocityCompressed)
		{
			if (smoothSync.isSyncingXAngularVelocity)
			{
				writer.Write(HalfHelper.Compress(state.angularVelocity.x));
			}
			if (smoothSync.isSyncingYAngularVelocity)
			{
				writer.Write(HalfHelper.Compress(state.angularVelocity.y));
			}
			if (smoothSync.isSyncingZAngularVelocity)
			{
				writer.Write(HalfHelper.Compress(state.angularVelocity.z));
			}
		}
		else
		{
			if (smoothSync.isSyncingXAngularVelocity)
			{
				writer.Write(state.angularVelocity.x);
			}
			if (smoothSync.isSyncingYAngularVelocity)
			{
				writer.Write(state.angularVelocity.y);
			}
			if (smoothSync.isSyncingZAngularVelocity)
			{
				writer.Write(state.angularVelocity.z);
			}
		}
	}

	public override void Deserialize(NetworkReader reader)
	{
		byte syncInformation = reader.ReadByte();
		bool flag = shouldSyncPosition(syncInformation);
		bool flag2 = shouldSyncRotation(syncInformation);
		bool flag3 = shouldSyncVelocity(syncInformation);
		bool flag4 = shouldSyncAngularVelocity(syncInformation);
		NetworkInstanceId netId = reader.ReadNetworkId();
		int num = (int)reader.ReadPackedUInt32();
		state.ownerTimestamp = (int)reader.ReadPackedUInt32();
		GameObject gameObject = null;
		gameObject = ((!NetworkServer.active) ? ClientScene.FindLocalObject(netId) : NetworkServer.FindLocalObject(netId));
		if (!gameObject)
		{
			Debug.LogWarning("Could not find target for network state message.");
			return;
		}
		smoothSync = gameObject.GetComponent<SmoothSync>();
		for (int i = 0; i < smoothSync.childObjectSmoothSyncs.Length; i++)
		{
			if (smoothSync.childObjectSmoothSyncs[i].syncIndex == num)
			{
				smoothSync = smoothSync.childObjectSmoothSyncs[i];
			}
		}
		if (!smoothSync)
		{
			Debug.LogWarning("Could not find target for network state message.");
			return;
		}
		if (flag)
		{
			if (smoothSync.isPositionCompressed)
			{
				if (smoothSync.isSyncingXPosition)
				{
					state.position.x = HalfHelper.Decompress(reader.ReadUInt16());
				}
				if (smoothSync.isSyncingYPosition)
				{
					state.position.y = HalfHelper.Decompress(reader.ReadUInt16());
				}
				if (smoothSync.isSyncingZPosition)
				{
					state.position.z = HalfHelper.Decompress(reader.ReadUInt16());
				}
			}
			else
			{
				if (smoothSync.isSyncingXPosition)
				{
					state.position.x = reader.ReadSingle();
				}
				if (smoothSync.isSyncingYPosition)
				{
					state.position.y = reader.ReadSingle();
				}
				if (smoothSync.isSyncingZPosition)
				{
					state.position.z = reader.ReadSingle();
				}
			}
		}
		else if (smoothSync.stateCount > 0)
		{
			state.position = smoothSync.stateBuffer[0].position;
		}
		else
		{
			state.position = smoothSync.getPosition();
		}
		if (flag3)
		{
			if (smoothSync.isVelocityCompressed)
			{
				if (smoothSync.isSyncingXVelocity)
				{
					state.velocity.x = HalfHelper.Decompress(reader.ReadUInt16());
				}
				if (smoothSync.isSyncingYVelocity)
				{
					state.velocity.y = HalfHelper.Decompress(reader.ReadUInt16());
				}
				if (smoothSync.isSyncingZVelocity)
				{
					state.velocity.z = HalfHelper.Decompress(reader.ReadUInt16());
				}
			}
			else
			{
				if (smoothSync.isSyncingXVelocity)
				{
					state.velocity.x = reader.ReadSingle();
				}
				if (smoothSync.isSyncingYVelocity)
				{
					state.velocity.y = reader.ReadSingle();
				}
				if (smoothSync.isSyncingZVelocity)
				{
					state.velocity.z = reader.ReadSingle();
				}
			}
		}
		else
		{
			state.velocity = Vector3.zero;
		}
		if (flag2)
		{
			Vector3 euler = default(Vector3);
			if (smoothSync.isRotationCompressed)
			{
				if (smoothSync.isSyncingXRotation)
				{
					euler.x = HalfHelper.Decompress(reader.ReadUInt16());
				}
				if (smoothSync.isSyncingYRotation)
				{
					euler.y = HalfHelper.Decompress(reader.ReadUInt16());
				}
				if (smoothSync.isSyncingZRotation)
				{
					euler.z = HalfHelper.Decompress(reader.ReadUInt16());
				}
				state.rotation = Quaternion.Euler(euler);
			}
			else
			{
				if (smoothSync.isSyncingXRotation)
				{
					euler.x = reader.ReadSingle();
				}
				if (smoothSync.isSyncingYRotation)
				{
					euler.y = reader.ReadSingle();
				}
				if (smoothSync.isSyncingZRotation)
				{
					euler.z = reader.ReadSingle();
				}
				state.rotation = Quaternion.Euler(euler);
			}
		}
		else if (smoothSync.stateCount > 0)
		{
			state.rotation = smoothSync.stateBuffer[0].rotation;
		}
		else
		{
			state.rotation = smoothSync.getRotation();
		}
		if (flag4)
		{
			if (smoothSync.isAngularVelocityCompressed)
			{
				if (smoothSync.isSyncingXAngularVelocity)
				{
					state.angularVelocity.x = HalfHelper.Decompress(reader.ReadUInt16());
				}
				if (smoothSync.isSyncingYAngularVelocity)
				{
					state.angularVelocity.y = HalfHelper.Decompress(reader.ReadUInt16());
				}
				if (smoothSync.isSyncingZAngularVelocity)
				{
					state.angularVelocity.z = HalfHelper.Decompress(reader.ReadUInt16());
				}
			}
			else
			{
				if (smoothSync.isSyncingXAngularVelocity)
				{
					state.angularVelocity.x = reader.ReadSingle();
				}
				if (smoothSync.isSyncingYAngularVelocity)
				{
					state.angularVelocity.y = reader.ReadSingle();
				}
				if (smoothSync.isSyncingZAngularVelocity)
				{
					state.angularVelocity.z = reader.ReadSingle();
				}
			}
		}
		else
		{
			state.angularVelocity = Vector3.zero;
		}
	}

	private byte encodeSyncInformation(bool sendPosition, bool sendRotation, bool sendVelocity, bool sendAngularVelocity)
	{
		byte b = 0;
		if (sendPosition)
		{
			b |= positionMask;
		}
		if (sendRotation)
		{
			b |= rotationMask;
		}
		if (sendVelocity)
		{
			b |= velocityMask;
		}
		if (sendAngularVelocity)
		{
			b |= angularVelocityMask;
		}
		return b;
	}

	private bool shouldSyncPosition(byte syncInformation)
	{
		if ((syncInformation & positionMask) == positionMask)
		{
			return true;
		}
		return false;
	}

	private bool shouldSyncRotation(byte syncInformation)
	{
		if ((syncInformation & rotationMask) == rotationMask)
		{
			return true;
		}
		return false;
	}

	private bool shouldSyncVelocity(byte syncInformation)
	{
		if ((syncInformation & velocityMask) == velocityMask)
		{
			return true;
		}
		return false;
	}

	private bool shouldSyncAngularVelocity(byte syncInformation)
	{
		if ((syncInformation & angularVelocityMask) == angularVelocityMask)
		{
			return true;
		}
		return false;
	}

	private SyncInfo assignSyncInfo(int syncPositionRotationInfo)
	{
		return syncPositionRotationInfo switch
		{
			3 => SyncInfo.BOTH, 
			2 => SyncInfo.ROTATION, 
			1 => SyncInfo.POSITION, 
			_ => SyncInfo.NONE, 
		};
	}
}
