using System;
using System.Collections;
using System.Collections.Generic;
using GameEvent;
using I2.Loc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZoomCamera : MonoBehaviour, IGameEventListener
{
	public enum DeadZoneMode
	{
		ACTUAL,
		FORCE_ON,
		FORCE_OFF
	}

	protected bool InventoryAdjustMode;

	public float InventoryAspectRatio = 0.714f;

	public float cameraAspect;

	public bool DebugShowCalculated;

	public bool DebugShowFrame;

	public bool DebugShowFrameWithBuffer;

	public bool DebugShowUnadjustedFrame;

	public bool DebugShowBoundary;

	public bool DebugShowDeadZone;

	public bool manualControls;

	public bool manualZoom;

	public float manualControlSpeed = 0.1f;

	public bool UseBuffer = true;

	public float TopBuffer;

	public float BottomBuffer;

	public float LeftBuffer;

	public float RightBuffer;

	protected float UnitTopBuffer;

	protected float UnitBottomBuffer;

	protected float UnitLeftBuffer;

	protected float UnitRightBuffer;

	public float inventoryLeftBuffer;

	protected float currentLeftBuffer;

	public float MinFrameHeight;

	public bool DontAdjustFrameHeight;

	public float MinFrameHeightMoving = 25f;

	public float MinFrameHeightStill = 10f;

	public float BuildMinFrameHeightMoving = 25f;

	public float BuildMinFrameHeightStill = 10f;

	public float MovingToMoveSpeed;

	public float MovingToStillSpeed;

	public float differenceThreshold;

	protected float difference;

	protected float MinFrameWidth;

	private float initialMinFrameHeight;

	private float initialMinFrameHeightMoving;

	private float initialMinFrameHeightStill;

	protected GameControl.GamePhase currentPhase;

	public Camera useCamera;

	public Animator CameraShaker;

	public Bounds DeadZone;

	public Vector2 DeadZonePercentage;

	public bool UseDeadZone;

	public bool smoothFollowCamOn;

	public bool unitBuffer;

	public float MaxFollowSpeed;

	protected Vector3 approachPercent = Vector3.zero;

	public Vector3 minApproachPercent;

	public Vector3 maxApproachPercent;

	public Vector3 DistForMaxApproach;

	protected Vector3 distance;

	protected List<Transform> targets = new List<Transform>(32);

	protected List<Collider2D> boxes = new List<Collider2D>(32);

	protected List<Character> characters = new List<Character>(8);

	protected List<Cursor> cursors = new List<Cursor>(8);

	protected bool followTarget = true;

	protected Bounds boundary;

	protected Bounds frame;

	protected bool paused;

	protected float lastManualFOV;

	protected Vector3 lastManualPosition;

	public Vector4 ManualMoveA;

	public Vector4 ManualMoveB;

	protected float manualMoveTime = 3f;

	protected bool manualMoveHappening;

	protected Vector3 lastAverageOfTargets;

	protected Vector3 currentAverageOfTargets;

	public Transform defaultCenterStart;

	public static Camera CurrentZoomCamera;

	protected static bool localOnly;

	protected bool forceAllPlayers;

	protected bool freeFormCamEnabled;

	public float Width { get; protected set; }

	public float Height { get; protected set; }

	public float DistanceToFrame => ((Vector2)(frame.center - base.transform.position)).magnitude;

	public static bool LocalOnly
	{
		get
		{
			return localOnly;
		}
		set
		{
			if (CurrentZoomCamera != null)
			{
				CurrentZoomCamera.GetComponent<ZoomCamera>().SetLocalOnly(value);
			}
			else
			{
				localOnly = value;
			}
		}
	}

	public static float GlobalCameraTime { get; protected set; }

	public static float LocalCameraTime { get; protected set; }

	public static void ResetTimers()
	{
		GlobalCameraTime = 0f;
		LocalCameraTime = 0f;
	}

	private void Awake()
	{
		initialMinFrameHeight = MinFrameHeight;
		initialMinFrameHeightMoving = MinFrameHeightMoving;
		initialMinFrameHeightStill = MinFrameHeightStill;
	}

	public void SetFrameSizes(float minFrameHeight)
	{
		initialMinFrameHeight = minFrameHeight;
		MinFrameHeight = initialMinFrameHeight * Modifiers.GetInstance().CharacterSizeZoomMultiplier;
		UpdateFrameSizes();
	}

	private void Start()
	{
		frame = GetFrame(UseBuffer);
		if (targets.Count > 0)
		{
			base.transform.position = frame.center;
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, -250f);
		}
		if ((bool)defaultCenterStart)
		{
			base.transform.position = defaultCenterStart.position;
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, -250f);
		}
		UpdateFrameSizes();
		Height = useCamera.orthographicSize;
		Width = Height * useCamera.aspect;
		frame = GetFrame(UseBuffer);
		useCamera.fieldOfView = GetFOV();
		ChangeListener(adding: true);
		currentLeftBuffer = LeftBuffer;
		CurrentZoomCamera = GetComponent<Camera>();
		GetFreeFormCamEnabled();
	}

	private void GetFreeFormCamEnabled()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-freeCam")
			{
				freeFormCamEnabled = true;
			}
		}
	}

	private void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	protected void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<NoteBookDisplayEvent>(this, adding);
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
	}

	private void UpdateFrameSizes()
	{
		float characterSizeZoomMultiplier = Modifiers.GetInstance().CharacterSizeZoomMultiplier;
		MinFrameHeightMoving = initialMinFrameHeightMoving * characterSizeZoomMultiplier;
		MinFrameHeightStill = initialMinFrameHeightStill * characterSizeZoomMultiplier;
		MinFrameWidth = useCamera.aspect * MinFrameHeight;
	}

	private void Update()
	{
		UpdateFrameSizes();
		if (freeFormCamEnabled)
		{
			if (Input.GetKeyDown(KeyCode.Minus))
			{
				DateTime now = DateTime.Now;
				int debugStutterAmount = GameSettings.GetInstance().DebugStutterAmount;
				int num = debugStutterAmount % 1000;
				int num2 = debugStutterAmount / 1000;
				TimeSpan timeSpan = now - now;
				while (timeSpan.Milliseconds < num || timeSpan.Seconds < num2)
				{
					timeSpan = DateTime.Now - now;
				}
			}
			if (Input.GetKeyDown(KeyCode.Backslash))
			{
				manualControls = !manualControls;
			}
			if (Input.GetKeyDown(KeyCode.Slash))
			{
				manualZoom = !manualZoom;
			}
			if (Input.GetKeyDown(KeyCode.F10))
			{
				if (lastManualPosition.magnitude > 0.1f)
				{
					base.transform.position = lastManualPosition;
				}
				if (lastManualFOV > 0.1f)
				{
					useCamera.fieldOfView = lastManualFOV;
				}
			}
			if (Input.GetKeyDown(KeyCode.F9))
			{
				lastManualFOV = useCamera.fieldOfView;
				lastManualPosition = base.transform.position;
			}
			if (manualControls)
			{
				Vector3 vector = default(Vector3);
				float num3 = 0f;
				if (Input.GetKey(KeyCode.L))
				{
					vector.x = manualControlSpeed;
				}
				if (Input.GetKey(KeyCode.J))
				{
					vector.x = 0f - manualControlSpeed;
				}
				if (Input.GetKey(KeyCode.I))
				{
					vector.y = manualControlSpeed;
				}
				if (Input.GetKey(KeyCode.K))
				{
					vector.y = 0f - manualControlSpeed;
				}
				if (Input.GetKey(KeyCode.U))
				{
					num3 = manualControlSpeed / 3f;
				}
				if (Input.GetKey(KeyCode.O))
				{
					num3 = (0f - manualControlSpeed) / 3f;
				}
				if (Input.GetKeyDown(KeyCode.LeftBracket))
				{
					manualControlSpeed *= 0.9f;
				}
				if (Input.GetKeyDown(KeyCode.RightBracket))
				{
					manualControlSpeed *= 1.11111f;
				}
				base.transform.position = base.transform.position + vector;
				useCamera.fieldOfView += num3;
				if (Input.GetKeyDown(KeyCode.Insert))
				{
					ManualMoveA = new Vector4(base.transform.position.x, base.transform.position.y, base.transform.position.z, useCamera.fieldOfView);
				}
				if (Input.GetKeyDown(KeyCode.Delete))
				{
					ManualMoveB = new Vector4(base.transform.position.x, base.transform.position.y, base.transform.position.z, useCamera.fieldOfView);
				}
				if (Input.GetKeyDown(KeyCode.PageUp))
				{
					manualMoveTime += 0.5f;
					if (manualMoveTime <= 0f)
					{
						manualMoveTime = 0.1f;
					}
				}
				if (Input.GetKeyDown(KeyCode.PageDown))
				{
					manualMoveTime -= 0.5f;
					if (manualMoveTime <= 0f)
					{
						manualMoveTime = 0.1f;
					}
				}
				if (Input.GetKeyDown(KeyCode.Home))
				{
					if (manualMoveHappening)
					{
						StopCoroutine("ManualCameraMove");
						manualMoveHappening = false;
					}
					else
					{
						StartCoroutine("ManualCameraMove");
					}
				}
				return;
			}
			if (manualZoom)
			{
				float num4 = 0f;
				if (Input.GetKey(KeyCode.U))
				{
					num4 = manualControlSpeed / 3f;
				}
				if (Input.GetKey(KeyCode.O))
				{
					num4 = (0f - manualControlSpeed) / 3f;
				}
				useCamera.fieldOfView += num4;
				return;
			}
		}
		if (UseDeadZone)
		{
			Bounds bounds = GetFrame(withBuffer: false, adjusted: true, DeadZoneMode.FORCE_OFF);
			DeadZone.extents = new Vector3(bounds.extents.x * DeadZonePercentage.x, bounds.extents.y * DeadZonePercentage.y, 0f);
			DeadZone.center = base.transform.position;
		}
		cameraAspect = useCamera.aspect;
		if (LocalOnly)
		{
			LocalCameraTime += Time.unscaledDeltaTime;
		}
		else
		{
			GlobalCameraTime += Time.unscaledDeltaTime;
		}
	}

	private IEnumerator ManualCameraMove()
	{
		if (!manualMoveHappening)
		{
			manualMoveHappening = true;
			float t = 0f;
			while (t < manualMoveTime)
			{
				t += Time.unscaledDeltaTime;
				Vector4 vector = Vector4.Lerp(ManualMoveA, ManualMoveB, GameSettings.GetInstance().SmoothManualCameraMove.Evaluate(t / manualMoveTime));
				base.transform.position = vector;
				useCamera.fieldOfView = vector.w;
				yield return null;
			}
			manualMoveHappening = false;
		}
	}

	private void FixedUpdate()
	{
		if (manualControls || paused || (Time.timeScale < 0.01f && !freeFormCamEnabled))
		{
			return;
		}
		if (!DontAdjustFrameHeight)
		{
			currentAverageOfTargets = Vector3.zero;
			float num = 0f;
			foreach (Transform target in targets)
			{
				if (!(target == null))
				{
					currentAverageOfTargets += target.position;
					Character component;
					if ((bool)(component = target.GetComponent<Character>()))
					{
						num += component.moveViewCurrent.left;
						num += component.moveViewCurrent.right;
						num += component.moveViewCurrent.down;
						num += component.moveViewCurrent.up;
					}
				}
			}
			foreach (Collider2D box in boxes)
			{
				if (box != null)
				{
					currentAverageOfTargets += box.transform.position;
				}
			}
			difference = (currentAverageOfTargets - lastAverageOfTargets).magnitude;
			if (currentPhase == GameControl.GamePhase.PLACE)
			{
				if (difference > 0.01f || num > 0.01f)
				{
					MinFrameHeight = Mathf.MoveTowards(MinFrameHeight, BuildMinFrameHeightMoving, MovingToMoveSpeed);
				}
				else
				{
					MinFrameHeight = Mathf.MoveTowards(MinFrameHeight, BuildMinFrameHeightStill, MovingToStillSpeed);
				}
			}
			else if (difference > 0.01f || num > 0.01f)
			{
				MinFrameHeight = Mathf.MoveTowards(MinFrameHeight, MinFrameHeightMoving, MovingToMoveSpeed);
			}
			else
			{
				MinFrameHeight = Mathf.MoveTowards(MinFrameHeight, MinFrameHeightStill, MovingToStillSpeed);
			}
			lastAverageOfTargets = currentAverageOfTargets;
		}
		frame = GetFrame(UseBuffer);
		Vector3 center = frame.center;
		float fOV = GetFOV();
		if (smoothFollowCamOn)
		{
			Vector3 vector = center - base.transform.position;
			if (!float.IsNaN(frame.extents.y))
			{
				vector.z = frame.extents.y - GetCameraView().extents.y;
				distance = vector;
				if (DistForMaxApproach.sqrMagnitude < float.Epsilon)
				{
					approachPercent = maxApproachPercent;
				}
				else
				{
					if (DistForMaxApproach.x == 0f)
					{
						approachPercent.x = maxApproachPercent.x;
					}
					else
					{
						approachPercent.x = Mathf.Lerp(minApproachPercent.x, maxApproachPercent.x, Mathf.Abs(vector.x / DistForMaxApproach.x));
					}
					if (DistForMaxApproach.y == 0f)
					{
						approachPercent.y = maxApproachPercent.y;
					}
					else
					{
						approachPercent.y = Mathf.Lerp(minApproachPercent.y, maxApproachPercent.y, Mathf.Abs(vector.y / DistForMaxApproach.y));
					}
					if (DistForMaxApproach.z == 0f)
					{
						approachPercent.z = maxApproachPercent.z;
					}
					else
					{
						approachPercent.z = Mathf.Lerp(minApproachPercent.z, maxApproachPercent.z, Mathf.Abs(vector.z / DistForMaxApproach.z));
					}
				}
			}
			float num2 = (fOV - useCamera.fieldOfView) * approachPercent.z;
			if (!manualZoom)
			{
				float num3 = useCamera.fieldOfView + num2 * Time.fixedDeltaTime;
				if (!float.IsNaN(num3))
				{
					useCamera.fieldOfView = num3;
				}
				else
				{
					Debug.LogWarning("Tried to set camera FOV to NaN");
				}
			}
			Vector3 vector2 = Vector3.Scale(vector, approachPercent);
			float num4 = GameSettings.GetInstance().MaxFollowSpeedBasedOnZoomModifier.Evaluate(useCamera.fieldOfView);
			if (vector2.magnitude > MaxFollowSpeed * num4 * Time.fixedDeltaTime)
			{
				vector2 = vector2.normalized * MaxFollowSpeed * Time.fixedDeltaTime * num4;
			}
			if (vector2.magnitude > vector.magnitude)
			{
				vector2 = vector;
			}
			if (!followTarget)
			{
				vector2 = Vector3.zero;
			}
			vector2.z = 0f;
			Vector3 position = base.transform.position + vector2;
			if (!float.IsNaN(position.x) && !float.IsNaN(position.y))
			{
				base.transform.position = position;
			}
			return;
		}
		base.transform.position = Vector3.MoveTowards(base.transform.position, frame.center, MaxFollowSpeed * Time.fixedDeltaTime);
		if (!manualZoom)
		{
			float num5 = Mathf.MoveTowards(useCamera.fieldOfView, fOV, MaxFollowSpeed * 57.29578f * Time.fixedDeltaTime);
			if (!float.IsNaN(num5))
			{
				useCamera.fieldOfView = num5;
			}
		}
	}

	public void SnapToTarget()
	{
		base.transform.position = frame.center;
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, -250f);
		useCamera.fieldOfView = GetFOV();
	}

	public void ZipToTarget(float speed, float zoomSpeed)
	{
		if (smoothFollowCamOn)
		{
			StartCoroutine(zipToTarget(speed, zoomSpeed));
		}
	}

	private IEnumerator zipToTarget(float speed, float zoomSpeed)
	{
		float originalSpeed = MaxFollowSpeed;
		Vector3 originalDist = DistForMaxApproach;
		DistForMaxApproach = new Vector3(0.1f, 0.1f, zoomSpeed);
		while (((Vector2)distance).sqrMagnitude > 1f)
		{
			MaxFollowSpeed = 400f;
			yield return null;
		}
		MaxFollowSpeed = originalSpeed;
		DistForMaxApproach = originalDist;
	}

	public void SetBounds(Bounds bounds)
	{
		boundary = bounds;
		frame = GetFrame();
	}

	public Bounds GetBounds()
	{
		return boundary;
	}

	public void AddTarget(Character character)
	{
		if (!characters.Contains(character))
		{
			characters.Add(character);
			if ((character.hasAuthority || !LocalOnly || forceAllPlayers) && !targets.Contains(character.transform))
			{
				targets.Add(character.transform);
			}
		}
	}

	public void AddTarget(Cursor cursor)
	{
		if (!cursors.Contains(cursor))
		{
			cursors.Add(cursor);
			if ((cursor.hasAuthority || !localOnly || forceAllPlayers) && !boxes.Contains(cursor.BoundingBox))
			{
				boxes.Add(cursor.BoundingBox);
			}
		}
	}

	public void AddTarget(Transform target)
	{
		if (!targets.Contains(target))
		{
			targets.Add(target);
		}
	}

	public void AddTarget(Collider2D box)
	{
		if (!boxes.Contains(box))
		{
			boxes.Add(box);
		}
	}

	public void RemoveTarget(Character character)
	{
		if (characters.Contains(character))
		{
			characters.Remove(character);
			if (targets.Contains(character.transform))
			{
				targets.Remove(character.transform);
			}
		}
	}

	public void RemoveTarget(Cursor cursor)
	{
		if (cursors.Contains(cursor))
		{
			cursors.Remove(cursor);
			if (boxes.Contains(cursor.BoundingBox))
			{
				boxes.Remove(cursor.BoundingBox);
			}
		}
	}

	public void RemoveTarget(Transform target)
	{
		if (targets.Contains(target))
		{
			targets.Remove(target);
		}
	}

	public void RemoveTarget(Collider2D box)
	{
		if (boxes.Contains(box))
		{
			boxes.Remove(box);
		}
	}

	public bool HasTarget(Character character)
	{
		return characters.Contains(character);
	}

	public bool HasTarget(Cursor cursor)
	{
		return cursors.Contains(cursor);
	}

	public bool HasTarget(Transform target)
	{
		return targets.Contains(target);
	}

	public bool HasTarget(Collider2D target)
	{
		return boxes.Contains(target);
	}

	public bool CameraIsInBounds()
	{
		Bounds cameraView = GetCameraView();
		if (boundary.Contains(cameraView.min))
		{
			return boundary.Contains(cameraView.max);
		}
		return false;
	}

	public void ClearTargets()
	{
		targets.Clear();
		boxes.Clear();
		characters.Clear();
		cursors.Clear();
	}

	public void ClearTransformTargets()
	{
		targets.Clear();
	}

	private void ClearNullElements<T>(List<T> list)
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] == null)
			{
				list.SwapRemove(num);
			}
		}
	}

	private void cleanupTargets()
	{
		ClearNullElements(targets);
		ClearNullElements(boxes);
		ClearNullElements(characters);
		ClearNullElements(cursors);
	}

	public void ForceFrameUpdate()
	{
		frame = GetFrame();
	}

	public Bounds GetFrame(bool withBuffer = true, bool adjusted = true, DeadZoneMode deadZoneMode = DeadZoneMode.ACTUAL)
	{
		cleanupTargets();
		MinFrameWidth = useCamera.aspect * MinFrameHeight;
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = num;
		float num4 = num2;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 1f;
		if (targets.Count + boxes.Count == 0)
		{
			num = boundary.min.x;
			num2 = boundary.max.x;
			num3 = boundary.min.y;
			num4 = boundary.max.y;
		}
		else
		{
			foreach (Transform target in targets)
			{
				if (target == null)
				{
					continue;
				}
				Vector3 position = target.position;
				if (position.x != -1000f || position.y != -1000f)
				{
					if (position.x < num)
					{
						num = position.x;
					}
					if (position.x > num2)
					{
						num2 = position.x;
					}
					if (position.y < num3)
					{
						num3 = position.y;
					}
					if (position.y > num4)
					{
						num4 = position.y;
					}
					Character component;
					if ((bool)(component = target.GetComponent<Character>()))
					{
						num5 += component.moveViewCurrent.left;
						num6 += component.moveViewCurrent.right;
						num7 += component.moveViewCurrent.down;
						num8 += component.moveViewCurrent.up;
					}
				}
			}
			num9 = 1f;
			foreach (Collider2D box in boxes)
			{
				if (!(box == null))
				{
					Bounds bounds = box.bounds;
					if (bounds.min.x < num)
					{
						num = bounds.min.x;
					}
					if (bounds.max.x > num2)
					{
						num2 = bounds.max.x;
					}
					if (bounds.min.y < num3)
					{
						num3 = bounds.min.y;
					}
					if (bounds.max.y > num4)
					{
						num4 = bounds.max.y;
					}
					Cursor componentInParent;
					if (GameSettings.GetInstance().GameMode == GameState.GameMode.FREEPLAY && (bool)(componentInParent = box.GetComponentInParent<Cursor>()) && componentInParent.extraZoomCurrent > num9)
					{
						num9 = componentInParent.extraZoomCurrent;
					}
				}
			}
		}
		float num10 = num2 - num;
		float num11 = num4 - num3;
		float num12 = num10 * currentLeftBuffer;
		float num13 = num10 * RightBuffer;
		float num14 = num11 * TopBuffer;
		float num15 = num11 * BottomBuffer;
		float num16 = 0f;
		if ((UseDeadZone && deadZoneMode == DeadZoneMode.ACTUAL) || deadZoneMode == DeadZoneMode.FORCE_ON)
		{
			Vector3 vector = new Vector3(DeadZone.min.x, DeadZone.min.y, 0f);
			Vector3 vector2 = new Vector3(DeadZone.max.x, DeadZone.max.y, 0f);
			if (withBuffer)
			{
				vector.x += num12;
				vector.y += num15;
				vector2.x -= num13;
				vector2.y -= num14;
			}
			if (unitBuffer)
			{
				vector.x += UnitLeftBuffer;
				vector.y += UnitBottomBuffer;
				vector2.x -= UnitRightBuffer;
				vector2.y -= UnitTopBuffer;
			}
			if (DeadZone.min.x < num)
			{
				num16 = num - DeadZone.min.x;
				num = DeadZone.min.x;
			}
			if (DeadZone.max.x > num2)
			{
				num2 = DeadZone.max.x;
			}
			if (DeadZone.min.y < num3)
			{
				num3 = DeadZone.min.y;
			}
			if (DeadZone.max.y > num4)
			{
				num4 = DeadZone.max.y;
			}
		}
		if (unitBuffer)
		{
			num12 += UnitLeftBuffer;
			num13 += UnitRightBuffer;
			num14 += UnitTopBuffer;
			num15 += UnitBottomBuffer;
		}
		if (withBuffer)
		{
			num -= num12;
			num2 += num13;
			num3 -= num15;
			num4 += num14;
		}
		num -= num5;
		num2 += num6;
		num3 -= num7;
		num4 += num8;
		num10 = num2 - num;
		num11 = num4 - num3;
		float num17 = num + num10 / 2f;
		float num18 = num3 + num11 / 2f;
		num10 *= num9;
		num11 *= num9;
		num = num17 - num10 / 2f;
		num2 = num17 + num10 / 2f;
		num3 = num18 - num11 / 2f;
		num4 = num18 + num11 / 2f;
		if (num2 > boundary.max.x)
		{
			num2 = boundary.max.x;
		}
		if (num < boundary.min.x)
		{
			num = boundary.min.x;
		}
		if (num4 > boundary.max.y)
		{
			num4 = boundary.max.y;
		}
		if (num3 < boundary.min.y)
		{
			num3 = boundary.min.y;
		}
		float num19 = (num2 - num) * useCamera.aspect / (useCamera.aspect - InventoryAspectRatio) - (num2 - num + num16);
		if (InventoryAdjustMode)
		{
			if (Modifiers.GetInstance().CameraFlippedOnX)
			{
				num2 += num19;
			}
			else
			{
				num -= num19;
			}
		}
		num10 = num2 - num;
		num11 = num4 - num3;
		num17 = num + num10 / 2f;
		num18 = num3 + num11 / 2f;
		if (adjusted)
		{
			if (Mathf.Abs(num10 / num11) >= useCamera.aspect)
			{
				if (num10 < MinFrameWidth)
				{
					num10 = MinFrameWidth;
				}
				num11 = num10 / useCamera.aspect;
			}
			else
			{
				if (num11 < MinFrameHeight)
				{
					num11 = MinFrameHeight;
				}
				num10 = num11 * useCamera.aspect;
			}
		}
		if (!InventoryAdjustMode)
		{
			if (num17 - num10 / 2f < boundary.center.x - boundary.extents.x)
			{
				num17 = boundary.center.x - boundary.extents.x + num10 / 2f;
			}
			if (num17 + num10 / 2f > boundary.center.x + boundary.extents.x)
			{
				num17 = boundary.center.x + boundary.extents.x - num10 / 2f;
			}
			if (num10 > boundary.extents.x * 2f)
			{
				num17 = boundary.center.x;
			}
		}
		if (num18 - num11 / 2f < boundary.center.y - boundary.extents.y)
		{
			num18 = boundary.center.y - boundary.extents.y + num11 / 2f;
		}
		if (num18 + num11 / 2f > boundary.center.y + boundary.extents.y)
		{
			num18 = boundary.center.y + boundary.extents.y - num11 / 2f;
		}
		if (num11 > boundary.extents.y * 2f)
		{
			num18 = boundary.center.y;
		}
		frame = new Bounds(new Vector3(num17, num18, base.transform.position.z), new Vector3(num10, num11, 0f));
		return frame;
	}

	public Bounds GetCameraView()
	{
		if (useCamera == null)
		{
			return default(Bounds);
		}
		float num;
		float f;
		if (useCamera.orthographic)
		{
			num = useCamera.orthographicSize * 2f;
			f = useCamera.aspect * num;
		}
		else
		{
			num = Mathf.Tan(useCamera.fieldOfView / 2f * (MathF.PI / 180f)) * base.transform.position.z * 2f;
			f = num * useCamera.aspect;
		}
		return new Bounds(base.transform.position, new Vector3(Mathf.Abs(f), Mathf.Abs(num), 0f));
	}

	protected float GetFOV()
	{
		float num = Mathf.Atan(frame.extents.y / frame.center.z) * 2f;
		if (!float.IsNaN(num))
		{
			return Mathf.Abs(57.29578f * num);
		}
		return useCamera.fieldOfView;
	}

	public void AllowFollow(bool follow)
	{
		followTarget = follow;
	}

	public void shakeCamera()
	{
		if (CameraShaker != null)
		{
			float value = UnityEngine.Random.Range(0f, 1f);
			CameraShaker.SetFloat("Blend", value);
			if (CameraShaker.GetCurrentAnimatorStateInfo(0).IsName("NoShake"))
			{
				CameraShaker.SetTrigger("Shake");
			}
		}
		else
		{
			Debug.Log("No Camera Shaker");
		}
	}

	public void SetUnitBuffer(Level level)
	{
		if (level == null)
		{
			Debug.LogError("Level not set when setting camera UnitBuffer, preventing null exception");
			return;
		}
		UnitBottomBuffer = level.LevelUnitBottomBuffer;
		UnitLeftBuffer = level.LevelUnitLeftBuffer;
		UnitRightBuffer = level.LevelUnitRightBuffer;
		UnitTopBuffer = level.LevelUnitTopBuffer;
	}

	public void SetLocalOnly(bool localOnly)
	{
		setLocalOnly(localOnly, temp: false);
	}

	private void setLocalOnly(bool localOnly, bool temp)
	{
		if (ZoomCamera.localOnly != localOnly || temp)
		{
			UserMessageManager.UserMsgPriority priority = UserMessageManager.UserMsgPriority.lo;
			if (!temp && SceneManager.GetActiveScene().name == "TreeHouseLobby")
			{
				priority = UserMessageManager.UserMsgPriority.hi;
			}
			if (localOnly)
			{
				if (!temp)
				{
					UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Network/CameraFollows") + " " + LocalizationManager.GetTranslation("Network/LocalPlayers"), 2f, priority, tiedToCurrentScene: true);
				}
				untrackRemotePlayers();
			}
			else
			{
				if (!temp)
				{
					UserMessageManager.Instance.UserMessage(LocalizationManager.GetTranslation("Network/CameraFollows") + " " + LocalizationManager.GetTranslation("Network/AllPlayers"), 2f, priority, tiedToCurrentScene: true);
				}
				trackAllPlayers();
			}
		}
		if (!temp)
		{
			ZoomCamera.localOnly = localOnly;
		}
	}

	public void RecheckLocalOnly()
	{
		trackAllPlayers();
		if (localOnly && !forceAllPlayers)
		{
			untrackRemotePlayers();
		}
	}

	private void untrackRemotePlayers()
	{
		foreach (Character character in characters)
		{
			if (!character.hasAuthority && targets.Contains(character.transform))
			{
				targets.Remove(character.transform);
			}
		}
		foreach (Cursor cursor in cursors)
		{
			if (!cursor.hasAuthority && boxes.Contains(cursor.BoundingBox))
			{
				boxes.Remove(cursor.BoundingBox);
			}
		}
	}

	private void trackAllPlayers()
	{
		foreach (Character character in characters)
		{
			if (!targets.Contains(character.transform))
			{
				targets.Add(character.transform);
			}
		}
		foreach (Cursor cursor in cursors)
		{
			if (!boxes.Contains(cursor.BoundingBox))
			{
				boxes.Add(cursor.BoundingBox);
			}
		}
	}

	public bool ToggleLocalOnly()
	{
		SetLocalOnly(!localOnly);
		return localOnly;
	}

	public void ForceShowAllPlayer(bool showAll)
	{
		forceAllPlayers = showAll;
		if (forceAllPlayers)
		{
			setLocalOnly(localOnly: false, temp: true);
		}
		else
		{
			setLocalOnly(localOnly, temp: true);
		}
	}

	public bool AnyPlayersTracked()
	{
		foreach (Character character in characters)
		{
			if (HasTarget(character.transform))
			{
				return true;
			}
		}
		foreach (Cursor cursor in cursors)
		{
			if (HasTarget(cursor.BoundingBox))
			{
				return true;
			}
		}
		return false;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(NoteBookDisplayEvent))
		{
			if ((e as NoteBookDisplayEvent).Opened)
			{
				if (targets.Count > 1 || boxes.Count > 1)
				{
					InventoryAdjustMode = true;
				}
			}
			else
			{
				InventoryAdjustMode = false;
			}
		}
		if (type == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				paused = true;
			}
			else
			{
				paused = false;
			}
		}
		if (type == typeof(StartPhaseEvent))
		{
			StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
			currentPhase = startPhaseEvent.Phase;
		}
	}
}
