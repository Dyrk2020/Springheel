using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using GameEvent;
using UnityEngine;
using UnityEngine.Networking;

public class KrakenTrigger : NetworkBehaviour, IGameEventListener
{
	[SerializeField]
	private TentaclesManager tentaclesManager;

	[SerializeField]
	private int secondsBetweenActivations = 30;

	[SerializeField]
	private Color inactiveColor;

	private int _playerMask;

	private bool _simulated;

	private SpriteRenderer _sprite;

	private Vector3 _visiblePosition;

	private Color _baseColor;

	private bool _active;

	private NetworkIdentity _networkIdentity;

	private Coroutine _tweenCoroutine;

	private bool _isPlaying;

	private static int kRpcRpcTriggerTentacles;

	private void Awake()
	{
		_playerMask = LayerMask.GetMask("NonLocalPlayer", "Player");
		_sprite = GetComponent<SpriteRenderer>();
		_visiblePosition = base.transform.position;
		_networkIdentity = GetComponent<NetworkIdentity>();
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding: true);
		SetActive(active: false, skipAnim: true);
	}

	private void OnDestroy()
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding: false);
	}

	private void FixedUpdate()
	{
		if (_simulated || !_networkIdentity.isServer || !_active || !_isPlaying)
		{
			return;
		}
		Transform obj = base.transform;
		Vector3 position = obj.position;
		float x = obj.localScale.x;
		Vector2 point = new Vector2(position.x, position.y);
		DebugUtil.DrawCircle(obj.position, Vector3.up, 4f * x, Color.red);
		Collider2D collider2D = Physics2D.OverlapCircle(point, 4f * x, _playerMask);
		if (collider2D != null)
		{
			Character component = collider2D.transform.root.GetComponent<Character>();
			if (component != null && !component.Dead)
			{
				CallRpcTriggerTentacles(UnityEngine.Random.Range(0, int.MaxValue));
			}
		}
	}

	[ClientRpc]
	private void RpcTriggerTentacles(int randomSeed)
	{
		TriggerTentacles(randomSeed);
	}

	private async UniTaskVoid TriggerTentacles(int randomSeed)
	{
		if (_simulated)
		{
			return;
		}
		AkSoundEngine.PostEvent("SFX_Level_Islands_Bell_Hit", base.gameObject);
		StartTween(_visiblePosition + new Vector3(0f, 25f, 0f), 0.25f);
		_simulated = true;
		await tentaclesManager.TriggerTentacles(randomSeed);
		await UniTask.Delay(secondsBetweenActivations * 1000);
		if (_active)
		{
			StartTween(_visiblePosition, 1f, delegate
			{
				_simulated = false;
			});
			AkSoundEngine.PostEvent("SFX_Level_Islands_Bell_Spawn", base.gameObject);
		}
		else
		{
			_simulated = false;
		}
	}

	public void SetActive(bool active, bool skipAnim = false)
	{
		_simulated = !_active && _simulated;
		_active = active;
		if (skipAnim)
		{
			base.transform.position = (active ? _visiblePosition : (_visiblePosition + new Vector3(0f, 25f, 0f)));
			return;
		}
		if (_active)
		{
			AkSoundEngine.PostEvent("SFX_Level_Islands_Bell_Spawn", base.gameObject);
		}
		StartTween(active ? _visiblePosition : (_visiblePosition + new Vector3(0f, 25f, 0f)), 1f);
	}

	private void StartTween(Vector3 endPosition, float time, Action onComplete = null)
	{
		if (_tweenCoroutine != null)
		{
			StopCoroutine(_tweenCoroutine);
		}
		_tweenCoroutine = StartCoroutine(TweenTo(endPosition, time, onComplete));
	}

	private IEnumerator TweenTo(Vector3 endPosition, float time, Action onComplete = null)
	{
		float startTime = Time.time;
		Vector3 startPosition = base.transform.position;
		while (Time.time - startTime < time)
		{
			float t = (Time.time - startTime) / time;
			base.transform.position = Vector3.Lerp(startPosition, endPosition, t);
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		onComplete?.Invoke();
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		_isPlaying = e is StartPhaseEvent startPhaseEvent && startPhaseEvent.Phase == GameControl.GamePhase.PLAY;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcTriggerTentacles(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTriggerTentacles called on server.");
		}
		else
		{
			((KrakenTrigger)obj).RpcTriggerTentacles((int)reader.ReadPackedUInt32());
		}
	}

	public void CallRpcTriggerTentacles(int randomSeed)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcTriggerTentacles called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcTriggerTentacles);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)randomSeed);
		SendRPCInternal(networkWriter, 0, "RpcTriggerTentacles");
	}

	static KrakenTrigger()
	{
		kRpcRpcTriggerTentacles = 1771491709;
		NetworkBehaviour.RegisterRpcDelegate(typeof(KrakenTrigger), kRpcRpcTriggerTentacles, InvokeRpcRpcTriggerTentacles);
		NetworkCRC.RegisterBehaviour("KrakenTrigger", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
