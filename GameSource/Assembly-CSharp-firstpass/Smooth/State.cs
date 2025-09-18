using UnityEngine;
using UnityEngine.Networking;

namespace Smooth;

public class State
{
	public bool syncPosition = true;

	public bool syncRotation = true;

	public int ownerTimestamp;

	public Vector3 position;

	public Quaternion rotation;

	public Vector3 velocity;

	public Vector3 angularVelocity;

	public State()
	{
	}

	public State(State state)
	{
		syncPosition = state.syncPosition;
		syncRotation = state.syncRotation;
		ownerTimestamp = state.ownerTimestamp;
		position = state.position;
		rotation = state.rotation;
		velocity = state.velocity;
		angularVelocity = state.angularVelocity;
	}

	public State(SmoothSync smoothSyncScript)
	{
		ownerTimestamp = NetworkTransport.GetNetworkTimestamp();
		position = smoothSyncScript.getPosition();
		rotation = smoothSyncScript.getRotation();
		if (smoothSyncScript.hasRigdibody)
		{
			velocity = smoothSyncScript.rb.velocity;
			angularVelocity = smoothSyncScript.rb.angularVelocity;
		}
		else if (smoothSyncScript.hasRigidbody2D)
		{
			velocity = smoothSyncScript.rb2D.velocity;
			angularVelocity = new Vector3(0f, 0f, smoothSyncScript.rb2D.angularVelocity);
		}
		else
		{
			velocity = Vector3.zero;
			angularVelocity = Vector3.zero;
		}
	}

	public static State Lerp(State start, State end, float t)
	{
		return new State
		{
			position = Vector3.Lerp(start.position, end.position, t),
			rotation = Quaternion.Lerp(start.rotation, end.rotation, t),
			velocity = Vector3.Lerp(start.velocity, end.velocity, t),
			angularVelocity = Vector3.Lerp(start.angularVelocity, end.angularVelocity, t),
			ownerTimestamp = (int)Mathf.Lerp(start.ownerTimestamp, end.ownerTimestamp, t)
		};
	}
}
