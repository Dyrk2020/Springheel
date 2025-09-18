using System;
using GameEvent;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class LightGrid : NetworkBehaviour, IGameEventListener
{
	public BoxCollider2D GridSize;

	public LightCell LightCellPrefab;

	public float triggerRadius;

	private LightCell[,] cells;

	private Vector2 referenceCorner;

	private int width;

	private int height;

	private bool[] rpcCellArray;

	private static int kRpcRpcReportSolidCells;

	private void Awake()
	{
		Bounds bounds = GridSize.bounds;
		referenceCorner = new Vector2(Mathf.Ceil(bounds.min.x), Mathf.Ceil(bounds.min.y)) - (Vector2)base.transform.position;
		width = (int)(Mathf.Ceil(bounds.max.x - base.transform.position.x) - referenceCorner.x);
		height = (int)(Mathf.Ceil(bounds.max.y - base.transform.position.y) - referenceCorner.y);
		cells = new LightCell[width, height];
		rpcCellArray = new bool[width * height];
		for (int i = 0; i != width * height; i++)
		{
			int num = i % width;
			int num2 = (i - num) / width;
			LightCell lightCell = UnityEngine.Object.Instantiate(LightCellPrefab, base.transform);
			lightCell.transform.localPosition = new Vector3(referenceCorner.x + (float)num, referenceCorner.y + (float)num2, 0f);
			lightCell.CycleOffset = lightCell.transform.localPosition.magnitude / 5f;
			lightCell.xPos = num;
			lightCell.yPos = num2;
			lightCell.grid = this;
			cells[num, num2] = lightCell;
		}
	}

	private void Start()
	{
		GameEventManager.ChangeListener<LevelResetEvent>(this, adding: true);
		GameEventManager.ChangeListener<EndPhaseEvent>(this, adding: true);
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		CollisionTag component = collision.GetComponent<CollisionTag>();
		if (!(component != null) || !component.ContainsAllTags((TagComparer.Tag)160))
		{
			return;
		}
		Vector3 position = collision.transform.position;
		float num = position.x - GridSize.bounds.min.x;
		float num2 = position.y - GridSize.bounds.min.y;
		Character componentInParent = collision.GetComponentInParent<Character>();
		if (componentInParent != null && !componentInParent.CrouchingDown)
		{
			num2 += 0.5f;
		}
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				int num3 = Mathf.FloorToInt(num - (float)i * triggerRadius * Modifiers.GetInstance().CharacterLightCellTriggerRadiusMultiplier);
				int num4 = Mathf.FloorToInt(num2 - (float)j * triggerRadius * Modifiers.GetInstance().CharacterLightCellTriggerRadiusMultiplier);
				if (num3 >= 0 && num4 >= 0 && num3 < width && num4 < height)
				{
					LightCell lightCell = cells[num3, num4];
					if (lightCell != null && lightCell.CellState == LightCell.State.OFF)
					{
						lightCell.TurnOn(lightCell.DefaultColour);
					}
				}
			}
		}
	}

	public void NotifyNeighbours(LightCell cell)
	{
		bool lit = cell.CellState != LightCell.State.OFF;
		if (cell.xPos > 0)
		{
			cells[cell.xPos - 1, cell.yPos].SetNeighbour(LightCell.Neighbour.RIGHT, lit);
		}
		if (cell.yPos > 0)
		{
			cells[cell.xPos, cell.yPos - 1].SetNeighbour(LightCell.Neighbour.TOP, lit);
		}
		if (cell.xPos < width - 1)
		{
			cells[cell.xPos + 1, cell.yPos].SetNeighbour(LightCell.Neighbour.LEFT, lit);
		}
		if (cell.yPos < height - 1)
		{
			cells[cell.xPos, cell.yPos + 1].SetNeighbour(LightCell.Neighbour.BOTTOM, lit);
		}
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		Type type = e.GetType();
		if (type == typeof(LevelResetEvent))
		{
			for (int i = 0; i != width; i++)
			{
				for (int j = 0; j != height; j++)
				{
					cells[i, j].TurnOff();
					rpcCellArray[i + j * width] = false;
				}
			}
		}
		if (!(type == typeof(EndPhaseEvent)) || !base.hasAuthority || (e as EndPhaseEvent).Phase != GameControl.GamePhase.PLAY)
		{
			return;
		}
		for (int k = 0; k != width; k++)
		{
			for (int l = 0; l != height; l++)
			{
				LightCell lightCell = cells[k, l];
				rpcCellArray[k + l * width] = lightCell.CellState == LightCell.State.LIT;
			}
		}
		CallRpcReportSolidCells(rpcCellArray);
	}

	[ClientRpc]
	private void RpcReportSolidCells(bool[] cellsSolid)
	{
		AkSoundEngine.PostEvent("SFX_Level_Light_Solidifying", base.gameObject);
		for (int i = 0; i != width; i++)
		{
			for (int j = 0; j != height; j++)
			{
				LightCell lightCell = cells[i, j];
				if (cellsSolid[i + j * width])
				{
					lightCell.Solidify();
				}
				else
				{
					lightCell.TurnOff();
				}
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcReportSolidCells(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReportSolidCells called on server.");
		}
		else
		{
			((LightGrid)obj).RpcReportSolidCells(GeneratedNetworkCode._ReadArrayBoolean_None(reader));
		}
	}

	public void CallRpcReportSolidCells(bool[] cellsSolid)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcReportSolidCells called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcReportSolidCells);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayBoolean_None(networkWriter, cellsSolid);
		SendRPCInternal(networkWriter, 0, "RpcReportSolidCells");
	}

	static LightGrid()
	{
		kRpcRpcReportSolidCells = -35701172;
		NetworkBehaviour.RegisterRpcDelegate(typeof(LightGrid), kRpcRpcReportSolidCells, InvokeRpcRpcReportSolidCells);
		NetworkCRC.RegisterBehaviour("LightGrid", 0);
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
