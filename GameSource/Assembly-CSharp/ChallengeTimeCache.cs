using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ChallengeTimeCache : MonoBehaviour
{
	private class PostedScore
	{
		public List<string> playerIds;

		public float time;

		public bool allCoins;

		public string code;

		public int retryCount;
	}

	private const int maxScoreUploadRetryCount = 5;

	private Dictionary<string, List<PostedScore>> postedScoreList;

	private bool purging;

	private static ChallengeTimeCache instance;

	public static ChallengeTimeCache Instance
	{
		get
		{
			if (instance == null)
			{
				new GameObject("Challenge Time Cache").AddComponent<ChallengeTimeCache>();
			}
			return instance;
		}
	}

	public static bool HasDataLeftToUpload
	{
		get
		{
			if (!Instance.purging)
			{
				return Instance.postedScoreList.Count > 0;
			}
			return true;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		postedScoreList = new Dictionary<string, List<PostedScore>>();
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			if (postedScoreList.Count > 0)
			{
				Debug.LogError("ChallengeTimeCache: Some scores were not uploaded");
			}
			if (purging)
			{
				Debug.LogError("ChallengeTimeCache: Destroyed while scores were being purged");
			}
		}
	}

	public void PostDeferredChallengeTime(string levelCode, List<string> playerIds, float time, bool allCoins)
	{
		List<PostedScore> value = null;
		if (!postedScoreList.TryGetValue(levelCode, out value))
		{
			value = new List<PostedScore>();
			postedScoreList.Add(levelCode, value);
		}
		bool flag = true;
		foreach (PostedScore item in value)
		{
			if (SamePlayerIds(playerIds, item.playerIds) && item.allCoins == allCoins)
			{
				if (item.time > time)
				{
					item.time = time;
				}
				flag = false;
				break;
			}
		}
		if (flag)
		{
			value.Add(new PostedScore
			{
				playerIds = playerIds,
				time = time,
				code = levelCode,
				allCoins = allCoins
			});
		}
	}

	private bool SamePlayerIds(List<string> a, List<string> b)
	{
		if (a.Count == b.Count)
		{
			if (a == b)
			{
				return true;
			}
			for (int i = 0; i < a.Count; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private IEnumerator PurgeQueue()
	{
		purging = true;
		Queue<PostedScore> scoresToPurge = new Queue<PostedScore>();
		foreach (KeyValuePair<string, List<PostedScore>> postedScore in postedScoreList)
		{
			foreach (PostedScore item in postedScore.Value)
			{
				scoresToPurge.Enqueue(item);
			}
		}
		postedScoreList.Clear();
		while (scoresToPurge.Count > 0)
		{
			PostedScore score = scoresToPurge.Dequeue();
			bool queryResolved = false;
			GameSparksQuery query = GameSparksManager.Instance.CreateQuery();
			query.SubmitChallengeTime(score.code, score.playerIds.ToList(), score.time, score.allCoins, noUpdate: false);
			query.FinishListeners = (UnityAction<GameSparksQuery>)Delegate.Combine(query.FinishListeners, (UnityAction<GameSparksQuery>)delegate(GameSparksQuery q)
			{
				if (!q.HasError)
				{
					queryResolved = true;
				}
			});
			while (!query.IsDone)
			{
				yield return null;
			}
			if (!queryResolved)
			{
				score.retryCount++;
				if (score.retryCount < 5)
				{
					scoresToPurge.Enqueue(score);
				}
				else
				{
					Debug.LogWarning("Warning: Score for level code " + score.code + " was discarded after failing to upload " + 5 + " times");
				}
			}
			if (!GameSparksManager.Instance.Connected)
			{
				MergeBack(scoresToPurge);
				scoresToPurge.Clear();
			}
		}
		purging = false;
	}

	private void Update()
	{
		if (GameSparksManager.Instance.Connected && postedScoreList.Count > 0 && !purging)
		{
			StartCoroutine(PurgeQueue());
		}
	}

	private void MergeBack(Queue<PostedScore> scoresToMerge)
	{
		int num = 0;
		foreach (PostedScore item in scoresToMerge)
		{
			PostDeferredChallengeTime(item.code, item.playerIds, item.time, item.allCoins);
		}
		Debug.LogWarning("ChallengeTimeCache: Merged back " + num + " entries.");
	}
}
