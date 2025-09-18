using System;
using UnityEngine;

public class GDKInvitation
{
	private string mOriginalInviteUri;

	private string mInvitedUserXuid;

	private string mSenderXuid;

	private LobbyData mPendingLobbyInvitation;

	private string mConnectionString;

	public const string INVITE_URI = "inviteAccept";

	private const string INVITED_USER_KEY = "invitedUser=";

	private const string SENDER_KEY = "sender=";

	private const string CONNECTION_STRING_KEY = "connectionString=";

	public string OriginalInviteUri => mOriginalInviteUri;

	public string InvitedUserXuid => mInvitedUserXuid;

	public string SenderXuid => mSenderXuid;

	public GDKInvitation(string inviteUri)
	{
		mOriginalInviteUri = inviteUri;
		mInvitedUserXuid = ExtractParamFromInvite("invitedUser=", inviteUri);
		mSenderXuid = ExtractParamFromInvite("sender=", inviteUri);
		mConnectionString = ExtractParamFromInvite("connectionString=", inviteUri);
		Debug.Log("Decode with connection string " + mConnectionString);
		mPendingLobbyInvitation = LobbyData.Deserialize(mConnectionString);
		Debug.Log("Connection string decoded. Lobby code is " + GetLobbyCode());
	}

	public string GetLobbyCode()
	{
		if (mPendingLobbyInvitation == null)
		{
			return null;
		}
		return mPendingLobbyInvitation.lobbyId;
	}

	private string ExtractParamFromInvite(string key, string inviteUri)
	{
		int num = inviteUri.IndexOf(key, StringComparison.InvariantCulture) + key.Length;
		int num2 = inviteUri.IndexOf("&", num, StringComparison.InvariantCulture);
		string text = ((num2 != -1) ? inviteUri.Substring(num, num2 - num) : inviteUri.Substring(num));
		Debug.Log("From invite uri, " + key + text + ", full inviteUri was " + inviteUri);
		return text;
	}
}
