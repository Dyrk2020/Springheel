using UnityEngine.Networking;

public class MsgUnlockAvailable : MessageBase
{
	public UnLockInfo.UnlockType UnlockType;

	public Character.Animals AssociatedCharacter;

	public GameState.LevelName AssociatedLevel;

	public int OutfitNumber;

	public string DisplayName;

	public int connid;

	public int playerLocalNumber;

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((int)UnlockType);
		writer.Write((int)AssociatedCharacter);
		writer.Write((int)AssociatedLevel);
		writer.WritePackedUInt32((uint)OutfitNumber);
		writer.Write(DisplayName);
		writer.WritePackedUInt32((uint)connid);
		writer.WritePackedUInt32((uint)playerLocalNumber);
	}

	public override void Deserialize(NetworkReader reader)
	{
		UnlockType = (UnLockInfo.UnlockType)reader.ReadInt32();
		AssociatedCharacter = (Character.Animals)reader.ReadInt32();
		AssociatedLevel = (GameState.LevelName)reader.ReadInt32();
		OutfitNumber = (int)reader.ReadPackedUInt32();
		DisplayName = reader.ReadString();
		connid = (int)reader.ReadPackedUInt32();
		playerLocalNumber = (int)reader.ReadPackedUInt32();
	}
}
