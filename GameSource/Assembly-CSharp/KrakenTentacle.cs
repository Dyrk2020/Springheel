using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MyBox;
using UnityEngine;

public class KrakenTentacle : MonoBehaviour
{
	private static readonly int AttackType = Animator.StringToHash("state");

	[SerializeField]
	private Animator visualAnimator;

	[SerializeField]
	private float maximumPositionY;

	[SerializeField]
	private float minimumPositionY = -7f;

	[SerializeField]
	private Animator krakenSplashAnimator;

	public List<int> possibleAttackIndexes;

	[SerializeField]
	private Transform _pivot;

	private SyncedRandom _random;

	private Animator[] _allAnimators;

	[SerializeField]
	private int _currentAnimIndex;

	private int _side;

	private SyncedRandom Random => _random;

	private void Awake()
	{
		_allAnimators = GetComponentsInChildren<Animator>(includeInactive: true);
	}

	[ButtonMethod(ButtonMethodDrawOrder.AfterInspector)]
	public void StartTentacle()
	{
		AkSoundEngine.PostEvent("SFX_Level_Islands_Kraken_Tentacle_Enter", base.gameObject);
		Vector3 position = krakenSplashAnimator.transform.position;
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y = Random.Range(minimumPositionY, maximumPositionY);
		base.transform.localPosition = localPosition;
		krakenSplashAnimator.transform.position = position;
		visualAnimator.SetInteger(AttackType, _currentAnimIndex);
		krakenSplashAnimator.SetInteger(AttackType, 2);
		_pivot.localScale = new Vector3((_side == 1) ? 1 : (-1), 1f, 1f);
	}

	public void PreSelectAttack(int attackIndex)
	{
		_currentAnimIndex = attackIndex;
		_side = Random.Range(0, 2);
	}

	[ButtonMethod(ButtonMethodDrawOrder.AfterInspector)]
	public void PlayFeedback()
	{
		krakenSplashAnimator.SetInteger(AttackType, 1);
	}

	public void DoPause()
	{
		Animator[] allAnimators = _allAnimators;
		for (int i = 0; i < allAnimators.Length; i++)
		{
			allAnimators[i].speed = 0f;
		}
	}

	public void DoUnpause()
	{
		Animator[] allAnimators = _allAnimators;
		for (int i = 0; i < allAnimators.Length; i++)
		{
			allAnimators[i].speed = 1f;
		}
	}

	public async UniTaskVoid DoReset()
	{
		Animator[] allAnimators = _allAnimators;
		for (int i = 0; i < allAnimators.Length; i++)
		{
			allAnimators[i].gameObject.SetActive(value: false);
		}
		await UniTask.Delay(200);
		allAnimators = _allAnimators;
		for (int i = 0; i < allAnimators.Length; i++)
		{
			allAnimators[i].gameObject.SetActive(value: true);
		}
	}

	[ButtonMethod(ButtonMethodDrawOrder.AfterInspector)]
	public void DebugSetRandom()
	{
		SetRandom(new SyncedRandom(10));
	}

	public void SetRandom(SyncedRandom random)
	{
		_random = random;
	}
}
