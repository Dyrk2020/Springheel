using UnityEngine;

public class Tentacle : MonoBehaviour
{
	[SerializeField]
	private Transform tentacleTop;

	[SerializeField]
	private GameObject tentacleWarningPrefab;

	[SerializeField]
	private float baseStartRange = -2f;

	[SerializeField]
	private float baseEndRange = 2f;

	[SerializeField]
	private float topStartRange = -5f;

	[SerializeField]
	private float topEndRange = 5f;

	[SerializeField]
	private float baseSpeed = 1f;

	[SerializeField]
	private float topSpeed = 2f;

	[SerializeField]
	private float maximumPositionY;

	[SerializeField]
	private float minimumPositionY = -10.5f;

	[SerializeField]
	private float hiddenPositionY = -25f;

	[SerializeField]
	private float yPositionOscillation = 0.5f;

	[SerializeField]
	private float timeToShow = 1f;

	[SerializeField]
	private float timeToHide = 3f;

	[SerializeField]
	private float timeToSimulate = 20f;

	private SyncedRandom _random;

	private Transform _transform;

	private ParticleSystem _tentacleWarning;

	private float _baseSpeed;

	private float _topSpeed;

	private float _endPosition;

	private float _oscillationFactor;

	private float _simulationTime;

	private float _baseTime;

	private float _topTime;

	private float _startTime = float.MaxValue;

	private bool _simulating;

	private SyncedRandom Random => _random;

	private void Awake()
	{
		_transform = base.transform;
		GameObject gameObject = Object.Instantiate(tentacleWarningPrefab, base.transform.parent);
		_tentacleWarning = gameObject.GetComponent<ParticleSystem>();
		gameObject.SetActive(value: false);
	}

	public void StartTentacle()
	{
		_tentacleWarning.Stop(withChildren: true);
		_baseSpeed = baseSpeed * Random.Range(0.9f, 1.2f);
		_topSpeed = topSpeed * Random.Range(0.9f, 1.2f);
		_baseTime = Random.Range(0, 10);
		_topTime = Random.Range(-5, 5);
		_oscillationFactor = Random.Range(0.8f, 2f);
		_startTime = Time.time;
		_endPosition = Random.Range(minimumPositionY, maximumPositionY);
		_simulationTime = timeToSimulate * Random.Range(0.8f, 1.3f);
	}

	private void Update()
	{
		if (CheckIsSimulating())
		{
			HandleShow();
			HandleHide();
			OscillatePosition();
			UpdateTentacleTransform(_transform, _baseSpeed, baseStartRange, baseEndRange, ref _baseTime);
			UpdateTentacleTransform(tentacleTop, _topSpeed, topStartRange, topEndRange, ref _topTime);
		}
	}

	private bool CheckIsSimulating()
	{
		return _startTime + _simulationTime > Time.time;
	}

	private void OscillatePosition()
	{
		if (!(_startTime + timeToShow > Time.time) && !(_startTime + _simulationTime - timeToHide < Time.time))
		{
			Vector3 localPosition = base.transform.localPosition;
			float t = Mathf.Sin(Time.time * _oscillationFactor);
			float y = Mathf.Lerp(_endPosition - yPositionOscillation, _endPosition + yPositionOscillation, t);
			localPosition.y = y;
			base.transform.localPosition = localPosition;
		}
	}

	private void HandleShow()
	{
		if (!(_startTime + timeToShow < Time.time))
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition = new Vector3(localPosition.x, Mathf.Lerp(hiddenPositionY, _endPosition, (Time.time - _startTime) / timeToShow), localPosition.z);
			base.transform.localPosition = localPosition;
		}
	}

	private void HandleHide()
	{
		if (!(_startTime + _simulationTime - timeToHide > Time.time))
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition = new Vector3(localPosition.x, Mathf.Lerp(_endPosition, hiddenPositionY, (Time.time - _startTime - (_simulationTime - timeToHide)) / timeToHide), localPosition.z);
			base.transform.localPosition = localPosition;
		}
	}

	private void UpdateTentacleTransform(Transform trans, float speed, float rangeStart, float rangeEnd, ref float time)
	{
		time += Time.deltaTime * speed;
		float num = Mathf.Sin(time);
		num = (num + 1f) / 2f;
		float z = Mathf.Lerp(rangeStart, rangeEnd, num);
		trans.localRotation = Quaternion.Euler(0f, 0f, z);
	}

	public void ShowWarning()
	{
		Transform obj = _tentacleWarning.transform;
		Vector3 localPosition = _transform.localPosition;
		localPosition.y = 0.5f;
		obj.localPosition = localPosition;
		_tentacleWarning.gameObject.SetActive(value: true);
		_tentacleWarning.Play();
	}

	public void SetRandom(SyncedRandom random)
	{
		_random = random;
	}
}
