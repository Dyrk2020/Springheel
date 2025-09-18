using UnityEngine;

public struct ChatMessageDetails
{
	public Character.Animals Animal;

	public int NetworkNumber;

	public string UserName;

	public Color UserNameColor;

	public string Message;

	public Color MessageColor;

	public EmoteMeanings EmoteType;

	public bool isChatMessage;

	public string GSID;

	public string platformID;

	public LobbyPlayer.SocialPlatform platform;

	public ChatMessageDetails(Character.Animals animal, string userName, Color userNameColor, string message, EmoteMeanings emoteType, int networkNumber)
	{
		Animal = animal;
		UserName = userName;
		NetworkNumber = networkNumber;
		UserNameColor = userNameColor;
		Message = message;
		if (Message == null)
		{
			Message = "";
		}
		MessageColor = Color.white;
		EmoteType = emoteType;
		isChatMessage = true;
		GSID = null;
		platformID = null;
		platform = LobbyPlayer.SocialPlatform.Undefined;
	}

	public ChatMessageDetails(Character.Animals animal, string userName, Color userNameColor, string message, Color messageColor, EmoteMeanings emoteType, int networkNumber)
	{
		Animal = animal;
		UserName = userName;
		NetworkNumber = networkNumber;
		UserNameColor = userNameColor;
		Message = message;
		MessageColor = messageColor;
		EmoteType = emoteType;
		isChatMessage = true;
		GSID = null;
		platformID = null;
		platform = LobbyPlayer.SocialPlatform.Undefined;
	}
}
