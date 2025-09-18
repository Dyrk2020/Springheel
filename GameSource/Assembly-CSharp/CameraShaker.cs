using UnityEngine;

public class CameraShaker : MonoBehaviour
{
	public Transform targetTransform;

	public float shakeAmount = 0.1f;

	private Vector3 initialPosition;

	private float shakeTimer;

	private Animator _animator;

	private bool _isPaused;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void Start()
	{
		initialPosition = targetTransform.position;
	}

	private void Update()
	{
		if (shakeTimer > 0f && !_isPaused)
		{
			Vector3 vector = Random.insideUnitSphere * shakeAmount;
			targetTransform.position = initialPosition + vector;
			shakeTimer -= Time.deltaTime;
			if (shakeTimer <= 0f)
			{
				targetTransform.position = initialPosition;
				_animator.enabled = true;
			}
		}
	}

	public void PauseShake()
	{
		_isPaused = true;
	}

	public void UnPauseShake()
	{
		_isPaused = false;
	}

	public void ForceStop()
	{
		shakeTimer = 0f;
	}

	public void StartShake(float duration)
	{
		shakeTimer = duration;
		_animator.enabled = false;
	}
}
