using UnityEngine.Networking;

public class MsgSetCustomPortalInfo : MessageBase
{
	public GameState.PortalID PortalID;

	public GameState.LevelName targetLevel;

	public string snapshotName;

	public string code;

	public string authorGSID;

	public string authorDisplayName;

	public LobbyPlayer.SocialPlatform authorPlatform;

	public string authorPlatformID;

	public CustomLevelPortal.AuthorInfo AuthorInfo
	{
		get
		{
			if (authorGSID.NullOrEmpty())
			{
				return null;
			}
			return new CustomLevelPortal.AuthorInfo(authorGSID, authorDisplayName, authorPlatformID, authorPlatform);
		}
	}

	public override void Serialize(NetworkWriter writer)
	{
		writer.Write((int)PortalID);
		writer.Write((int)targetLevel);
		writer.Write(snapshotName);
		writer.Write(code);
		writer.Write(authorGSID);
		writer.Write(authorDisplayName);
		writer.Write((int)authorPlatform);
		writer.Write(authorPlatformID);
	}

	public override void Deserialize(NetworkReader reader)
	{
		PortalID = (GameState.PortalID)reader.ReadInt32();
		targetLevel = (GameState.LevelName)reader.ReadInt32();
		snapshotName = reader.ReadString();
		code = reader.ReadString();
		authorGSID = reader.ReadString();
		authorDisplayName = reader.ReadString();
		authorPlatform = (LobbyPlayer.SocialPlatform)reader.ReadInt32();
		authorPlatformID = reader.ReadString();
	}
}
