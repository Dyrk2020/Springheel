using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TentaclesManager : ActiveBlock
{
	[SerializeField]
	private Transform startXSpawnRange;

	[SerializeField]
	private Transform endXSpawnRange;

	[SerializeField]
	[Range(0f, 1f)]
	private float spawnZonePercentage = 0.6f;

	private SyncedRandom _random;

	private KrakenTentacle[] _tentacles;

	private CameraShaker _cameraShaker;

	private Dictionary<int, int> _usedAttacksCount = new Dictionary<int, int>();

	private List<KrakenTentacle> _shuffledTentacles = new List<KrakenTentacle>();

	public SyncedRandom Random => _random;

	protected override void Awake()
	{
		base.Awake();
		_tentacles = GetComponentsInChildren<KrakenTentacle>();
	}

	public override void Pause()
	{
		base.Pause();
		_cameraShaker.PauseShake();
		KrakenTentacle[] tentacles = _tentacles;
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i].DoPause();
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		_cameraShaker.UnPauseShake();
		KrakenTentacle[] tentacles = _tentacles;
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i].DoUnpause();
		}
	}

	public override void Reset()
	{
		if (_cameraShaker == null)
		{
			_cameraShaker = Object.FindObjectOfType<CameraShaker>();
		}
		_cameraShaker.ForceStop();
		KrakenTentacle[] tentacles = _tentacles;
		for (int i = 0; i < tentacles.Length; i++)
		{
			tentacles[i].DoReset();
		}
		base.Reset();
	}

	public async UniTask TriggerTentacles(int randomSeed)
	{
		_random = new SyncedRandom(randomSeed);
		if (_cameraShaker == null)
		{
			_cameraShaker = Object.FindObjectOfType<CameraShaker>();
		}
		_cameraShaker.shakeAmount = 0.2f;
		_cameraShaker.StartShake(4f);
		AkSoundEngine.PostEvent("SFX_Level_Islands_Kraken_Spawn", base.gameObject);
		_shuffledTentacles.Clear();
		_shuffledTentacles.AddRange(_tentacles);
		_random.ShuffleList(_shuffledTentacles);
		float num = (endXSpawnRange.position.x - startXSpawnRange.position.x) / (float)_tentacles.Length;
		_usedAttacksCount.Clear();
		for (int i = 0; i < _shuffledTentacles.Count; i++)
		{
			KrakenTentacle obj = _tentacles[i];
			KrakenTentacle krakenTentacle = _shuffledTentacles[i];
			krakenTentacle.SetRandom(Random);
			float x = startXSpawnRange.localPosition.x + num * (float)i + Random.Range(0f, num * spawnZonePercentage) + 1f - spawnZonePercentage / 2f;
			Transform obj2 = obj.transform;
			Vector3 localPosition = obj2.localPosition;
			localPosition.x = x;
			obj2.localPosition = localPosition;
			PreSelectAttack(krakenTentacle);
			krakenTentacle.PlayFeedback();
		}
		await UniTask.Delay(1500);
		foreach (KrakenTentacle shuffledTentacle in _shuffledTentacles)
		{
			shuffledTentacle.StartTentacle();
			await UniTask.Delay(Random.Range(500, 1200));
		}
	}

	private void PreSelectAttack(KrakenTentacle tentacle)
	{
		for (int i = 0; i < 10; i++)
		{
			List<int> possibleAttackIndexes = tentacle.possibleAttackIndexes;
			int count = possibleAttackIndexes.Count;
			int num = possibleAttackIndexes[Random.Range(0, count)];
			if (!_usedAttacksCount.ContainsKey(num) || _usedAttacksCount[num] < 2)
			{
				if (!_usedAttacksCount.ContainsKey(num))
				{
					_usedAttacksCount[num] = 0;
				}
				_usedAttacksCount[num]++;
				tentacle.PreSelectAttack(num);
				break;
			}
		}
	}
}
