using System.Runtime.InteropServices;
using GameEvent;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LevelUnlockCounter : NetworkBehaviour, IGameEventListener
{
	public Text LevelUnlockSign;

	public int MaxLevels;

	[SyncVar]
	private int levelCount;

	public int NetworklevelCount
	{
		get
		{
			return levelCount;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref levelCount, 1u);
		}
	}

	private void Start()
	{
		GameEventManager.ChangeListener<ResetDataEvent>(this, adding: true);
		GameEventManager.ChangeListener<CheatUnlockEvent>(this, adding: true);
		GameEventManager.ChangeListener<CheatUnlockHalfEvent>(this, adding: true);
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<ResetDataEvent>(this, adding: false);
		GameEventManager.ChangeListener<CheatUnlockEvent>(this, adding: false);
		GameEventManager.ChangeListener<CheatUnlockHalfEvent>(this, adding: false);
	}

	public void CountLevels()
	{
		bool[] values = StatTracker.Instance.GetSaveFileDataForMainUser().GetStat<StatBoolArray>("LevelsUnlocked").values;
		int num = 0;
		if (base.hasAuthority)
		{
			for (int i = 0; i != values.Length; i++)
			{
				if (values[i] && i != 10)
				{
					num++;
				}
			}
			if (num > MaxLevels)
			{
				num = MaxLevels;
			}
			NetworklevelCount = num;
		}
		LevelUnlockSign.text = levelCount + "/" + MaxLevels;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		CountLevels();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)levelCount);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)levelCount);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			levelCount = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			levelCount = (int)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
