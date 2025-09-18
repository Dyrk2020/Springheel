using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Smooth;

public class SmoothSync : NetworkBehaviour
{
	[Tooltip("Increasing will make interpolation more likely to be used, decreasing will make extrapolation more likely to be used. In seconds.")]
	public float interpolationBackTime = 0.1f;

	[Tooltip("How much time into the 'future' a non-owner is allowed to extrapolate. In seconds.")]
	public float extrapolationTimeLimit = 0.3f;

	[Tooltip("How much distance into the 'future' a non-owner is allowed to extrapolate. In distance units.")]
	public float extrapolationDistanceLimit = 0.3f;

	[Tooltip("A synced object's position is only sent if it is off from the last sent position by more than the threshold. In distance units.")]
	public float sendMovementThreshold = 0.001f;

	[Tooltip("A synced object's rotation is only sent if it is off from the last sent rotation by more than the threshold. In degrees.")]
	public float sendRotationThreshold = 0.001f;

	[Tooltip("A synced object's velocity is only sent if it is off from the last sent velocity by more than the threshold. In velocity units.")]
	public float sendVelocityThreshold = 0.001f;

	[Tooltip("A synced object's angular velocity is only sent if it is off from the last sent angular velocity by more than the threshold. In radians per second.")]
	public float sendAngularVelocityThreshold = 0.001f;

	[SerializeField]
	[Tooltip("A synced object's position is only updated if it is off from the target position by more than the threshold. In distance units.")]
	[FormerlySerializedAs("movementThreshold")]
	public float receivedMovementThreshold = 0.001f;

	[SerializeField]
	[Tooltip("A synced object's rotation is only updated if it is off from the target rotation by more than the threshold. In degrees.")]
	public float receivedRotationThreshold = 0.001f;

	[Tooltip("If a synced object's position is more than snapThreshold units from the target position it will jump to the target position immediately instead of lerping. In distance units.")]
	public float positionSnapThreshold = 5f;

	[Tooltip("If a synced object's position is more than snapThreshold units from the target position it will jump to the target position immediately instead of lerping. In degrees.")]
	public float rotationSnapThreshold = 60f;

	[Range(0f, 1f)]
	[Tooltip("How fast to lerp to the new target state.")]
	public float lerpSpeed = 0.2f;

	[Tooltip("Fine tune how position is synced.")]
	public SyncMode syncPosition;

	[Tooltip("Fine tune how rotation is synced.")]
	public SyncMode syncRotation;

	[Tooltip("Fine tune how velocity is synced.")]
	public SyncMode syncVelocity;

	[Tooltip("Fine tune how angular velocity is synced.")]
	public SyncMode syncAngularVelocity;

	[Tooltip("Compress floats to save bandwidth.")]
	public bool isPositionCompressed = true;

	[Tooltip("Compress floats to save bandwidth.")]
	public bool isRotationCompressed = true;

	[Tooltip("Compress floats to save bandwidth.")]
	public bool isVelocityCompressed = true;

	[Tooltip("Compress floats to save bandwidth.")]
	public bool isAngularVelocityCompressed = true;

	[Tooltip("How many times per second to send network updates")]
	public float sendRate = 30f;

	[Tooltip("The channel to send network updates on.")]
	public int networkChannel = 1;

	[Tooltip("Set this to sync a child object, leave blank to sync this object. Must have one blank to sync the parent in order to sync children.")]
	public GameObject childObjectToSync;

	[NonSerialized]
	public bool hasChildObject;

	[NonSerialized]
	public State[] stateBuffer = new State[10];

	[NonSerialized]
	public int stateCount;

	[NonSerialized]
	public Rigidbody rb;

	[NonSerialized]
	public bool hasRigdibody;

	[NonSerialized]
	public Rigidbody2D rb2D;

	[NonSerialized]
	public bool hasRigidbody2D;

	private bool skipLerp;

	private bool dontLerp;

	[NonSerialized]
	public float lastTeleportOwnerTime;

	[NonSerialized]
	public float lastTimeStateWasSent;

	[NonSerialized]
	public float lastTimeStateWasReceived;

	[NonSerialized]
	public Vector3 lastPositionWhenStateWasSent;

	[NonSerialized]
	public Quaternion lastRotationWhenStateWasSent = Quaternion.identity;

	[NonSerialized]
	public Vector3 lastVelocityWhenStateWasSent;

	[NonSerialized]
	public Vector3 lastAngularVelocityWhenStateWasSent;

	private NetworkIdentity netID;

	[NonSerialized]
	public GameObject realObjectToSync;

	[NonSerialized]
	public int syncIndex;

	[NonSerialized]
	public SmoothSync[] childObjectSmoothSyncs = new SmoothSync[0];

	private State extrapolationEndState;

	private float extrapolationStopTime;

	private int _ownerTime;

	private float lastTimeOwnerTimeWasSet;

	public bool isSyncingXPosition
	{
		get
		{
			if (syncPosition != SyncMode.XYZ && syncPosition != SyncMode.XY && syncPosition != SyncMode.XZ)
			{
				return syncPosition == SyncMode.X;
			}
			return true;
		}
	}

	public bool isSyncingYPosition
	{
		get
		{
			if (syncPosition != SyncMode.XYZ && syncPosition != SyncMode.XY && syncPosition != SyncMode.YZ)
			{
				return syncPosition == SyncMode.Y;
			}
			return true;
		}
	}

	public bool isSyncingZPosition
	{
		get
		{
			if (syncPosition != SyncMode.XYZ && syncPosition != SyncMode.XZ && syncPosition != SyncMode.YZ)
			{
				return syncPosition == SyncMode.Z;
			}
			return true;
		}
	}

	public bool isSyncingXRotation
	{
		get
		{
			if (syncRotation != SyncMode.XYZ && syncRotation != SyncMode.XY && syncRotation != SyncMode.XZ)
			{
				return syncRotation == SyncMode.X;
			}
			return true;
		}
	}

	public bool isSyncingYRotation
	{
		get
		{
			if (syncRotation != SyncMode.XYZ && syncRotation != SyncMode.XY && syncRotation != SyncMode.YZ)
			{
				return syncRotation == SyncMode.Y;
			}
			return true;
		}
	}

	public bool isSyncingZRotation
	{
		get
		{
			if (syncRotation != SyncMode.XYZ && syncRotation != SyncMode.XZ && syncRotation != SyncMode.YZ)
			{
				return syncRotation == SyncMode.Z;
			}
			return true;
		}
	}

	public bool isSyncingXVelocity
	{
		get
		{
			if (syncVelocity != SyncMode.XYZ && syncVelocity != SyncMode.XY && syncVelocity != SyncMode.XZ)
			{
				return syncVelocity == SyncMode.X;
			}
			return true;
		}
	}

	public bool isSyncingYVelocity
	{
		get
		{
			if (syncVelocity != SyncMode.XYZ && syncVelocity != SyncMode.XY && syncVelocity != SyncMode.YZ)
			{
				return syncVelocity == SyncMode.Y;
			}
			return true;
		}
	}

	public bool isSyncingZVelocity
	{
		get
		{
			if (syncVelocity != SyncMode.XYZ && syncVelocity != SyncMode.XZ && syncVelocity != SyncMode.YZ)
			{
				return syncVelocity == SyncMode.Z;
			}
			return true;
		}
	}

	public bool isSyncingXAngularVelocity
	{
		get
		{
			if (syncAngularVelocity != SyncMode.XYZ && syncAngularVelocity != SyncMode.XY && syncAngularVelocity != SyncMode.XZ)
			{
				return syncAngularVelocity == SyncMode.X;
			}
			return true;
		}
	}

	public bool isSyncingYAngularVelocity
	{
		get
		{
			if (syncAngularVelocity != SyncMode.XYZ && syncAngularVelocity != SyncMode.XY && syncAngularVelocity != SyncMode.YZ)
			{
				return syncAngularVelocity == SyncMode.Y;
			}
			return true;
		}
	}

	public bool isSyncingZAngularVelocity
	{
		get
		{
			if (syncAngularVelocity != SyncMode.XYZ && syncAngularVelocity != SyncMode.XZ && syncAngularVelocity != SyncMode.YZ)
			{
				return syncAngularVelocity == SyncMode.Z;
			}
			return true;
		}
	}

	public int approximateNetworkTimeOnOwner
	{
		get
		{
			return _ownerTime + (int)((Time.realtimeSinceStartup - lastTimeOwnerTimeWasSet) * 1000f);
		}
		set
		{
			_ownerTime = value;
			lastTimeOwnerTimeWasSet = Time.realtimeSinceStartup;
		}
	}

	private void Awake()
	{
		netID = GetComponent<NetworkIdentity>();
		rb = GetComponent<Rigidbody>();
		rb2D = GetComponent<Rigidbody2D>();
		if ((bool)rb && childObjectToSync == null)
		{
			hasRigdibody = true;
		}
		if ((bool)rb2D && childObjectToSync == null)
		{
			hasRigidbody2D = true;
			if (syncVelocity != SyncMode.NONE)
			{
				syncVelocity = SyncMode.XY;
			}
			if (syncAngularVelocity != SyncMode.NONE)
			{
				syncAngularVelocity = SyncMode.Z;
			}
		}
		if ((!rb && !rb2D) || (bool)childObjectToSync)
		{
			syncVelocity = SyncMode.NONE;
			syncAngularVelocity = SyncMode.NONE;
		}
		if ((bool)childObjectToSync)
		{
			realObjectToSync = childObjectToSync;
			hasChildObject = true;
			bool flag = false;
			childObjectSmoothSyncs = GetComponents<SmoothSync>();
			for (int i = 0; i < childObjectSmoothSyncs.Length; i++)
			{
				if (!childObjectSmoothSyncs[i].childObjectToSync)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				Debug.LogError("Must have one SmoothSync script with unassigned childObjectToSync to sync the parent object");
			}
		}
		else
		{
			realObjectToSync = base.gameObject;
			int num = 0;
			childObjectSmoothSyncs = GetComponents<SmoothSync>();
			for (int j = 0; j < childObjectSmoothSyncs.Length; j++)
			{
				childObjectSmoothSyncs[j].syncIndex = num;
				num++;
			}
		}
	}

	public Vector3 getPosition()
	{
		if (hasChildObject)
		{
			return realObjectToSync.transform.localPosition;
		}
		return realObjectToSync.transform.position;
	}

	public Quaternion getRotation()
	{
		if (hasChildObject)
		{
			return realObjectToSync.transform.localRotation;
		}
		return realObjectToSync.transform.rotation;
	}

	public void setPosition(Vector3 position, bool isTeleporting)
	{
		if (hasChildObject)
		{
			realObjectToSync.transform.localPosition = position;
			return;
		}
		if (hasRigdibody && !isTeleporting)
		{
			rb.MovePosition(position);
		}
		if (hasRigidbody2D && !isTeleporting)
		{
			rb2D.MovePosition(position);
		}
		else
		{
			realObjectToSync.transform.position = position;
		}
	}

	public void setRotation(Quaternion rotation, bool isTeleporting)
	{
		if (hasChildObject)
		{
			realObjectToSync.transform.localRotation = rotation;
			return;
		}
		if (hasRigdibody && !isTeleporting)
		{
			rb.MoveRotation(rotation);
		}
		if (hasRigidbody2D && !isTeleporting)
		{
			rb2D.MoveRotation(rotation.eulerAngles.z);
		}
		else
		{
			realObjectToSync.transform.rotation = rotation;
		}
	}

	private void FixedUpdate()
	{
		if (!base.hasAuthority)
		{
			setInterpolationPosition();
		}
	}

	private void setInterpolationPosition()
	{
		if (stateCount == 0)
		{
			return;
		}
		bool flag = false;
		State targetState;
		if (dontLerp)
		{
			targetState = new State(this);
		}
		else
		{
			float num = (float)approximateNetworkTimeOnOwner - interpolationBackTime * 1000f;
			if (stateCount > 1 && (float)stateBuffer[0].ownerTimestamp > num)
			{
				interpolate(num, out targetState);
			}
			else
			{
				flag = !extrapolate(num, out targetState);
			}
		}
		float num2 = lerpSpeed;
		Mathf.Clamp01(num2);
		if (skipLerp)
		{
			num2 = 1f;
			skipLerp = false;
			dontLerp = false;
		}
		else if (dontLerp)
		{
			stateCount = 0;
			num2 = 1f;
		}
		if (!flag || (!hasRigdibody && !hasRigidbody2D))
		{
			bool flag2 = false;
			float num3 = Vector3.Distance(getPosition(), targetState.position);
			if (num3 > receivedMovementThreshold)
			{
				flag2 = true;
			}
			bool flag3 = false;
			float num4 = Quaternion.Angle(getRotation(), targetState.rotation);
			if (num4 > receivedRotationThreshold)
			{
				flag3 = true;
			}
			if (hasRigdibody && !rb.isKinematic)
			{
				if (flag2)
				{
					Vector3 velocity = rb.velocity;
					if (isSyncingXVelocity)
					{
						velocity.x = targetState.velocity.x;
					}
					if (isSyncingYVelocity)
					{
						velocity.y = targetState.velocity.y;
					}
					if (isSyncingZVelocity)
					{
						velocity.z = targetState.velocity.z;
					}
					rb.velocity = Vector3.Lerp(rb.velocity, velocity, num2);
				}
				else
				{
					rb.velocity = Vector3.zero;
					rb.angularVelocity = Vector3.zero;
				}
				if (flag3)
				{
					Vector3 angularVelocity = rb.angularVelocity;
					if (isSyncingXAngularVelocity)
					{
						angularVelocity.x = targetState.angularVelocity.x;
					}
					if (isSyncingYAngularVelocity)
					{
						angularVelocity.y = targetState.angularVelocity.y;
					}
					if (isSyncingZAngularVelocity)
					{
						angularVelocity.z = targetState.angularVelocity.z;
					}
					rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, angularVelocity, num2);
				}
				else
				{
					rb.angularVelocity = Vector3.zero;
				}
			}
			else if (hasRigidbody2D && !rb2D.isKinematic)
			{
				if (syncVelocity == SyncMode.XY)
				{
					if (flag2)
					{
						rb2D.velocity = Vector2.Lerp(rb2D.velocity, targetState.velocity, num2);
					}
					else
					{
						rb2D.velocity = Vector2.zero;
					}
				}
				if (syncAngularVelocity == SyncMode.Z)
				{
					if (flag3)
					{
						rb2D.angularVelocity = Mathf.Lerp(rb2D.angularVelocity, targetState.angularVelocity.z, num2);
					}
					else
					{
						rb2D.angularVelocity = 0f;
					}
				}
			}
			if (syncPosition != SyncMode.NONE && flag2)
			{
				float t = num2;
				bool isTeleporting = false;
				if (num3 > positionSnapThreshold)
				{
					t = 1f;
					isTeleporting = true;
				}
				Vector3 position = getPosition();
				if (isSyncingXPosition)
				{
					position.x = targetState.position.x;
				}
				if (isSyncingYPosition)
				{
					position.y = targetState.position.y;
				}
				if (isSyncingZPosition)
				{
					position.z = targetState.position.z;
				}
				setPosition(Vector3.Lerp(getPosition(), position, t), isTeleporting);
			}
			if (syncRotation != SyncMode.NONE && flag3)
			{
				float t2 = num2;
				bool isTeleporting2 = false;
				if (num4 > rotationSnapThreshold)
				{
					t2 = 1f;
					isTeleporting2 = true;
				}
				Vector3 eulerAngles = getRotation().eulerAngles;
				if (isSyncingXRotation)
				{
					eulerAngles.x = targetState.rotation.eulerAngles.x;
				}
				if (isSyncingYRotation)
				{
					eulerAngles.y = targetState.rotation.eulerAngles.y;
				}
				if (isSyncingZRotation)
				{
					eulerAngles.z = targetState.rotation.eulerAngles.z;
				}
				Quaternion b = Quaternion.Euler(eulerAngles);
				setRotation(Quaternion.Lerp(getRotation(), b, t2), isTeleporting2);
			}
		}
		else if (Vector3.Distance(stateBuffer[0].position, realObjectToSync.transform.position) >= extrapolationDistanceLimit)
		{
			if (hasRigdibody)
			{
				rb.velocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
			}
			if (hasRigidbody2D)
			{
				rb2D.velocity = Vector2.zero;
				rb2D.angularVelocity = 0f;
			}
		}
	}

	private void interpolate(float interpolationTime, out State targetState)
	{
		int i;
		for (i = 0; i < stateCount && !((float)stateBuffer[i].ownerTimestamp <= interpolationTime); i++)
		{
		}
		if (i == stateCount)
		{
			i--;
		}
		State state = stateBuffer[Mathf.Max(i - 1, 0)];
		State state2 = stateBuffer[i];
		float t = (interpolationTime - (float)state2.ownerTimestamp) / (float)(state.ownerTimestamp - state2.ownerTimestamp);
		targetState = State.Lerp(state2, state, t);
	}

	private bool extrapolate(float interpolationTime, out State targetState)
	{
		targetState = new State(stateBuffer[0]);
		float num = (interpolationTime - (float)targetState.ownerTimestamp) / 1000f;
		if (syncVelocity == SyncMode.NONE || targetState.velocity.magnitude < sendVelocityThreshold)
		{
			return true;
		}
		if ((hasRigdibody && !rb.isKinematic) || (hasRigidbody2D && !rb2D.isKinematic))
		{
			for (float num2 = 0f; num2 < num; num2 += Time.fixedDeltaTime)
			{
				if (num2 > extrapolationTimeLimit)
				{
					if (extrapolationStopTime < lastTimeStateWasReceived)
					{
						extrapolationEndState = targetState;
					}
					extrapolationStopTime = Time.realtimeSinceStartup;
					targetState = extrapolationEndState;
					return false;
				}
				float num3 = Mathf.Min(Time.fixedDeltaTime, num - num2);
				targetState.position += targetState.velocity * num3;
				if (hasRigdibody && rb.useGravity)
				{
					targetState.velocity += Physics.gravity * num3;
				}
				else if (hasRigidbody2D)
				{
					targetState.velocity += Physics.gravity * rb2D.gravityScale * num3;
				}
				if (hasRigdibody)
				{
					targetState.velocity -= targetState.velocity * num3 * rb.drag;
				}
				else if (hasRigidbody2D)
				{
					targetState.velocity -= targetState.velocity * num3 * rb2D.drag;
				}
				Quaternion quaternion = Quaternion.AngleAxis(num3 * targetState.angularVelocity.magnitude * 57.29578f, targetState.angularVelocity);
				targetState.rotation = quaternion * targetState.rotation;
				if (Vector3.Distance(stateBuffer[0].position, targetState.position) >= extrapolationDistanceLimit)
				{
					extrapolationEndState = targetState;
					extrapolationStopTime = Time.realtimeSinceStartup;
					targetState = extrapolationEndState;
					return false;
				}
			}
		}
		return true;
	}

	public void addState(State state)
	{
		if (stateCount > 1 && state.ownerTimestamp < stateBuffer[0].ownerTimestamp)
		{
			Debug.LogWarning("Received state out of order for: " + realObjectToSync.name);
			return;
		}
		lastTimeStateWasReceived = Time.realtimeSinceStartup;
		for (int num = stateBuffer.Length - 1; num >= 1; num--)
		{
			stateBuffer[num] = stateBuffer[num - 1];
		}
		stateBuffer[0] = state;
		stateCount = Mathf.Min(stateCount + 1, stateBuffer.Length);
	}

	public void stopLerping()
	{
		dontLerp = true;
	}

	public void restartLerping()
	{
		if (dontLerp)
		{
			skipLerp = true;
		}
	}

	public void clearBuffer()
	{
		stateCount = 0;
	}

	public void teleport(int networkTimestamp, Vector3 pos, Quaternion rot)
	{
		lastTeleportOwnerTime = networkTimestamp;
		setPosition(pos, isTeleporting: true);
		setRotation(rot, isTeleporting: true);
		clearBuffer();
		stopLerping();
	}

	public override void OnStartServer()
	{
		if (GetComponent<NetworkIdentity>().localPlayerAuthority)
		{
			if (!NetworkServer.handlers.ContainsKey(MsgType.SmoothSyncFromOwnerToServer))
			{
				NetworkServer.RegisterHandler(MsgType.SmoothSyncFromOwnerToServer, HandleSyncFromOwnerToServer);
			}
			if (NetworkManager.singleton.client != null && !NetworkManager.singleton.client.handlers.ContainsKey(MsgType.SmoothSyncFromServerToNonOwners))
			{
				NetworkManager.singleton.client.RegisterHandler(MsgType.SmoothSyncFromServerToNonOwners, HandleSyncFromServerToNonOwners);
			}
		}
	}

	public override void OnStartClient()
	{
		if (!NetworkServer.active && !NetworkManager.singleton.client.handlers.ContainsKey(MsgType.SmoothSyncFromServerToNonOwners))
		{
			NetworkManager.singleton.client.RegisterHandler(MsgType.SmoothSyncFromServerToNonOwners, HandleSyncFromServerToNonOwners);
		}
	}

	private void Update()
	{
		if (!base.hasAuthority || (!NetworkServer.active && !ClientScene.ready) || Time.realtimeSinceStartup - lastTimeStateWasSent < GetNetworkSendInterval() || (!shouldSendPosition() && !shouldSendRotation() && !shouldSendVelocity() && !shouldSendAngularVelocity()))
		{
			return;
		}
		lastTimeStateWasSent = Time.realtimeSinceStartup;
		NetworkState networkState = new NetworkState(this);
		if (NetworkServer.active)
		{
			SendStateToNonOwners(networkState);
			if (shouldSendPosition())
			{
				lastPositionWhenStateWasSent = getPosition();
			}
			bool flag = shouldSendVelocity();
			if (hasRigdibody)
			{
				if (flag)
				{
					lastVelocityWhenStateWasSent = rb.velocity;
				}
			}
			else if (hasRigidbody2D && flag)
			{
				lastVelocityWhenStateWasSent = rb2D.velocity;
			}
			bool flag2 = shouldSendAngularVelocity();
			if (hasRigdibody)
			{
				if (flag2)
				{
					lastAngularVelocityWhenStateWasSent = rb.angularVelocity;
				}
			}
			else if (hasRigidbody2D && flag2)
			{
				lastAngularVelocityWhenStateWasSent = new Vector3(0f, 0f, rb2D.angularVelocity);
			}
			if (shouldSendRotation())
			{
				lastRotationWhenStateWasSent = getRotation();
			}
		}
		else
		{
			NetworkManager.singleton.client.connection.SendByChannel(MsgType.SmoothSyncFromOwnerToServer, networkState, networkChannel);
		}
	}

	public bool shouldSendPosition()
	{
		if (Vector3.Distance(lastPositionWhenStateWasSent, getPosition()) > sendMovementThreshold)
		{
			return true;
		}
		return false;
	}

	public bool shouldSendVelocity()
	{
		if (hasRigdibody)
		{
			if (Vector3.Distance(lastVelocityWhenStateWasSent, rb.velocity) > sendVelocityThreshold)
			{
				return true;
			}
			return false;
		}
		if (hasRigidbody2D)
		{
			if (Vector2.Distance(lastVelocityWhenStateWasSent, rb2D.velocity) > sendVelocityThreshold)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool shouldSendRotation()
	{
		if (Quaternion.Angle(lastRotationWhenStateWasSent, getRotation()) > sendRotationThreshold)
		{
			return true;
		}
		return false;
	}

	public bool shouldSendAngularVelocity()
	{
		if (hasRigdibody)
		{
			if (Vector3.Distance(lastAngularVelocityWhenStateWasSent, rb.angularVelocity) > sendAngularVelocityThreshold)
			{
				return true;
			}
			return false;
		}
		if (hasRigidbody2D)
		{
			if (Mathf.Abs(lastAngularVelocityWhenStateWasSent.z - rb2D.angularVelocity) > sendAngularVelocityThreshold)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	[Server]
	private void SendStateToNonOwners(MessageBase state)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void Smooth.SmoothSync::SendStateToNonOwners(UnityEngine.Networking.MessageBase)' called on client");
			return;
		}
		for (int i = 0; i < NetworkServer.connections.Count; i++)
		{
			NetworkConnection networkConnection = NetworkServer.connections[i];
			if (networkConnection != null && networkConnection != netID.clientAuthorityOwner && networkConnection.hostId != -1 && networkConnection.isReady && isObservedByConnection(networkConnection))
			{
				networkConnection.SendByChannel(MsgType.SmoothSyncFromServerToNonOwners, state, networkChannel);
			}
		}
	}

	private bool isObservedByConnection(NetworkConnection conn)
	{
		for (int i = 0; i < netID.observers.Count; i++)
		{
			if (netID.observers[i] == conn)
			{
				return true;
			}
		}
		return false;
	}

	private static void HandleSyncFromServerToNonOwners(NetworkMessage msg)
	{
		NetworkState networkState = msg.ReadMessage<NetworkState>();
		if (networkState != null && networkState.smoothSync != null && !networkState.smoothSync.hasAuthority)
		{
			networkState.smoothSync.adjustOwnerTime(networkState.state.ownerTimestamp);
			if ((float)networkState.state.ownerTimestamp > networkState.smoothSync.lastTeleportOwnerTime)
			{
				networkState.smoothSync.restartLerping();
				networkState.smoothSync.addState(networkState.state);
			}
		}
	}

	private static void HandleSyncFromOwnerToServer(NetworkMessage msg)
	{
		NetworkState networkState = msg.ReadMessage<NetworkState>();
		if (networkState.smoothSync != null)
		{
			networkState.smoothSync.adjustOwnerTime(networkState.state.ownerTimestamp);
			networkState.smoothSync.SendStateToNonOwners(networkState);
			if ((float)networkState.state.ownerTimestamp > networkState.smoothSync.lastTeleportOwnerTime)
			{
				networkState.smoothSync.restartLerping();
				networkState.smoothSync.addState(networkState.state);
			}
		}
	}

	public override float GetNetworkSendInterval()
	{
		return 1f / sendRate;
	}

	public override int GetNetworkChannel()
	{
		return networkChannel;
	}

	private void adjustOwnerTime(int ownerTimestamp)
	{
		int num = 50;
		int num2 = Mathf.Abs(approximateNetworkTimeOnOwner - ownerTimestamp);
		if (approximateNetworkTimeOnOwner == 0 || num2 < num || num2 > num * 10)
		{
			approximateNetworkTimeOnOwner = ownerTimestamp;
		}
		else if (approximateNetworkTimeOnOwner < ownerTimestamp)
		{
			approximateNetworkTimeOnOwner += num;
		}
		else
		{
			approximateNetworkTimeOnOwner -= num;
		}
	}

	private void UNetVersion()
	{
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
