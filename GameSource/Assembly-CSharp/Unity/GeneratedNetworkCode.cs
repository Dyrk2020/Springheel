using System.Runtime.InteropServices;
using UnityEngine.Networking;

namespace Unity;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public class GeneratedNetworkCode
{
	public static void _ReadStructSyncListNetPlayerInfo_NetworkPlayerTracker(NetworkReader reader, NetworkPlayerTracker.SyncListNetPlayerInfo instance)
	{
		ushort num = reader.ReadUInt16();
		instance.Clear();
		for (ushort num2 = 0; num2 < num; num2++)
		{
			instance.AddInternal(instance.DeserializeItem(reader));
		}
	}

	public static void _WriteStructSyncListNetPlayerInfo_NetworkPlayerTracker(NetworkWriter writer, NetworkPlayerTracker.SyncListNetPlayerInfo value)
	{
		ushort count = value.Count;
		writer.Write(count);
		for (ushort num = 0; num < count; num++)
		{
			value.SerializeItem(writer, value.GetItem(num));
		}
	}

	public static void _ReadStructSyncListVoteState_TwitchChatClientState(NetworkReader reader, TwitchChatClientState.SyncListVoteState instance)
	{
		ushort num = reader.ReadUInt16();
		instance.Clear();
		for (ushort num2 = 0; num2 < num; num2++)
		{
			instance.AddInternal(instance.DeserializeItem(reader));
		}
	}

	public static void _WriteStructSyncListVoteState_TwitchChatClientState(NetworkWriter writer, TwitchChatClientState.SyncListVoteState value)
	{
		ushort count = value.Count;
		writer.Write(count);
		for (ushort num = 0; num < count; num++)
		{
			value.SerializeItem(writer, value.GetItem(num));
		}
	}

	public static bool[] _ReadArrayBoolean_None(NetworkReader reader)
	{
		int num = reader.ReadUInt16();
		if (num == 0)
		{
			return new bool[0];
		}
		bool[] array = new bool[num];
		for (int i = 0; i < num; i++)
		{
			ref bool reference = ref array[i];
			reference = reader.ReadBoolean();
		}
		return array;
	}

	public static void _WriteArrayBoolean_None(NetworkWriter writer, bool[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.Write(value[num]);
		}
	}

	public static int[] _ReadArrayInt32_None(NetworkReader reader)
	{
		int num = reader.ReadUInt16();
		if (num == 0)
		{
			return new int[0];
		}
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			ref int reference = ref array[i];
			reference = (int)reader.ReadPackedUInt32();
		}
		return array;
	}

	public static void _WriteArrayInt32_None(NetworkWriter writer, int[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.WritePackedUInt32((uint)value[num]);
		}
	}

	public static void _WritePlayedSnapshotInfo_LevelSelectController(NetworkWriter writer, LevelSelectController.PlayedSnapshotInfo value)
	{
		writer.Write((int)value.nextLevel);
		writer.Write(value.snapshotName);
		writer.Write(value.snapshotCode);
		writer.Write((int)value.snapshotType);
		writer.Write(value.authorID);
		writer.Write((int)value.authorPlatform);
		writer.Write(value.authorPlatformID);
		writer.Write(value.authorDisplayName);
	}

	public static LevelSelectController.PlayedSnapshotInfo _ReadPlayedSnapshotInfo_LevelSelectController(NetworkReader reader)
	{
		return new LevelSelectController.PlayedSnapshotInfo
		{
			nextLevel = (GameState.LevelName)reader.ReadInt32(),
			snapshotName = reader.ReadString(),
			snapshotCode = reader.ReadString(),
			snapshotType = (FeaturedQuickFilter.LevelTypes)reader.ReadInt32(),
			authorID = reader.ReadString(),
			authorPlatform = (LobbyPlayer.SocialPlatform)reader.ReadInt32(),
			authorPlatformID = reader.ReadString(),
			authorDisplayName = reader.ReadString()
		};
	}

	public static string[] _ReadArrayString_None(NetworkReader reader)
	{
		int num = reader.ReadUInt16();
		if (num == 0)
		{
			return new string[0];
		}
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			ref string reference = ref array[i];
			reference = reader.ReadString();
		}
		return array;
	}

	public static void _WriteArrayString_None(NetworkWriter writer, string[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.Write(value[num]);
		}
	}
}
