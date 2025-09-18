using System;
using System.Collections.Generic;
using BCGSComponents.DataModels;

namespace BCGSComponents;

public class CreateChallengeRequest : BCGSTypedRequest<CreateChallengeRequest, CreateChallengeResponse>
{
	public CreateChallengeRequest()
		: base("CreateChallengeRequest")
	{
	}

	public CreateChallengeRequest(BCGSInstance instance)
		: base(instance, "CreateChallengeRequest")
	{
	}

	protected override BCGSTypedResponse BuildResponse(BCGSObject response)
	{
		return new CreateChallengeResponse(response);
	}

	public CreateChallengeRequest SetAccessType(string accessType)
	{
		request.AddString("accessType", accessType);
		return this;
	}

	public CreateChallengeRequest SetAutoStartJoinedChallengeOnMaxPlayers(bool autoStartJoinedChallengeOnMaxPlayers)
	{
		request.AddBoolean("autoStartJoinedChallengeOnMaxPlayers", autoStartJoinedChallengeOnMaxPlayers);
		return this;
	}

	public CreateChallengeRequest SetChallengeMessage(string challengeMessage)
	{
		request.AddString("challengeMessage", challengeMessage);
		return this;
	}

	public CreateChallengeRequest SetChallengeShortCode(string challengeShortCode)
	{
		request.AddString("challengeShortCode", challengeShortCode);
		return this;
	}

	public CreateChallengeRequest SetCurrency1Wager(long currency1Wager)
	{
		request.AddNumber("currency1Wager", currency1Wager);
		return this;
	}

	public CreateChallengeRequest SetCurrency2Wager(long currency2Wager)
	{
		request.AddNumber("currency2Wager", currency2Wager);
		return this;
	}

	public CreateChallengeRequest SetCurrency3Wager(long currency3Wager)
	{
		request.AddNumber("currency3Wager", currency3Wager);
		return this;
	}

	public CreateChallengeRequest SetCurrency4Wager(long currency4Wager)
	{
		request.AddNumber("currency4Wager", currency4Wager);
		return this;
	}

	public CreateChallengeRequest SetCurrency5Wager(long currency5Wager)
	{
		request.AddNumber("currency5Wager", currency5Wager);
		return this;
	}

	public CreateChallengeRequest SetCurrency6Wager(long currency6Wager)
	{
		request.AddNumber("currency6Wager", currency6Wager);
		return this;
	}

	public CreateChallengeRequest SetCurrencyWagers(BCGSRequestData currencyWagers)
	{
		request.AddObject("currencyWagers", currencyWagers);
		return this;
	}

	public CreateChallengeRequest SetEligibilityCriteria(BCGSRequestData eligibilityCriteria)
	{
		request.AddObject("eligibilityCriteria", eligibilityCriteria);
		return this;
	}

	public CreateChallengeRequest SetEndTime(DateTime endTime)
	{
		request.AddDate("endTime", endTime);
		return this;
	}

	public CreateChallengeRequest SetExpiryTime(DateTime expiryTime)
	{
		request.AddDate("expiryTime", expiryTime);
		return this;
	}

	public CreateChallengeRequest SetMaxAttempts(long maxAttempts)
	{
		request.AddNumber("maxAttempts", maxAttempts);
		return this;
	}

	public CreateChallengeRequest SetMaxPlayers(long maxPlayers)
	{
		request.AddNumber("maxPlayers", maxPlayers);
		return this;
	}

	public CreateChallengeRequest SetMinPlayers(long minPlayers)
	{
		request.AddNumber("minPlayers", minPlayers);
		return this;
	}

	public CreateChallengeRequest SetSilent(bool silent)
	{
		request.AddBoolean("silent", silent);
		return this;
	}

	public CreateChallengeRequest SetStartTime(DateTime startTime)
	{
		request.AddDate("startTime", startTime);
		return this;
	}

	public CreateChallengeRequest SetUsersToChallenge(List<string> usersToChallenge)
	{
		request.AddStringList("usersToChallenge", usersToChallenge);
		return this;
	}
}
