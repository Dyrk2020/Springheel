using Unity;
using UnityEngine.Networking;

public class MsgPrepareToReloadScene : MessageBase
{
	public GameState.GameMode reloadToMode;

	public LevelSelectController.PlayedSnapshotInfo snapshotInfo;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((int)reloadToMode);
		GeneratedNetworkCode._WritePlayedSnapshotInfo_LevelSelectController(writer, snapshotInfo);
	}

	public override void Deserialize(NetworkReader reader)
	{
		reloadToMode = (GameState.GameMode)reader.ReadInt32();
		snapshotInfo = GeneratedNetworkCode._ReadPlayedSnapshotInfo_LevelSelectController(reader);
	}
}
