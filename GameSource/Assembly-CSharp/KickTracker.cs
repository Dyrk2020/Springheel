using System.Collections.Generic;

public class KickTracker
{
	private bool[][] votes;

	public KickTracker()
	{
		votes = new bool[4][];
		for (int i = 0; i != 4; i++)
		{
			votes[i] = new bool[4];
		}
	}

	public void SetVote(int votingPlayer, int targetPlayer, bool voteToKick)
	{
		votes[targetPlayer - 1][votingPlayer - 1] = voteToKick;
	}

	public bool GetVote(int votingPlayer, int targetPlayer)
	{
		return votes[targetPlayer - 1][votingPlayer - 1];
	}

	public void ClearPlayer(int player)
	{
		for (int i = 0; i != 4; i++)
		{
			votes[i][player - 1] = false;
			votes[player - 1][i] = false;
		}
	}

	public int CountVotes(int targetPlayer)
	{
		int num = 0;
		bool[] array = votes[targetPlayer - 1];
		for (int i = 0; i != 4; i++)
		{
			if (array[i])
			{
				num++;
			}
		}
		return num;
	}

	public IEnumerable<int> VotesFromNetworkNumber(int networkNumber)
	{
		int i = 0;
		while (i < 4)
		{
			if (i != networkNumber - 1 && votes[i][networkNumber - 1])
			{
				yield return i + 1;
			}
			int num = i + 1;
			i = num;
		}
	}
}
