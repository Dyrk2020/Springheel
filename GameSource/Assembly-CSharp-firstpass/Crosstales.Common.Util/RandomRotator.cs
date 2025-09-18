using UnityEngine;

namespace Crosstales.Common.Util;

[DisallowMultipleComponent]
public class RandomRotator : MonoBehaviour
{
	[Tooltip("Use intervals to change the rotation (default: true).")]
	public bool UseInterval = true;

	[Tooltip("Random change interval between min (= x) and max (= y) in seconds (default: x = 10, y = 20).")]
	public Vector2 ChangeInterval = new Vector2(10f, 20f);

	[Tooltip("Minimum rotation speed per axis (default: 5 for all axis).")]
	public Vector3 SpeedMin = new Vector3(5f, 5f, 5f);

	[Tooltip("Minimum rotation speed per axis (default: 15 for all axis).")]
	public Vector3 SpeedMax = new Vector3(15f, 15f, 15f);

	[Tooltip("Set the object to a random rotation at Start (default: false).")]
	public bool RandomRotationAtStart;

	[Tooltip("Random change interval per axis (default: true).")]
	public bool RandomChangeIntervalPerAxis = true;

	[Tooltip("Random direction per axis (default: true).")]
	public bool RandomDirectionPerAxis = true;

	private Transform _tf;

	private Vector3 _speed;

	private float _elapsedTime;

	private float _changeTime;

	private Vector3 _elapsedTimeAxis = Vector3.zero;

	private Vector3 _changeTimeAxis;

	private void Start()
	{
		_tf = base.transform;
		if (RandomChangeIntervalPerAxis)
		{
			_elapsedTimeAxis.x = (_changeTimeAxis.x = Random.Range(ChangeInterval.x, ChangeInterval.y));
			_elapsedTimeAxis.y = (_changeTimeAxis.y = Random.Range(ChangeInterval.x, ChangeInterval.y));
			_elapsedTimeAxis.z = (_changeTimeAxis.z = Random.Range(ChangeInterval.x, ChangeInterval.y));
		}
		else
		{
			_elapsedTime = (_changeTime = Random.Range(ChangeInterval.x, ChangeInterval.y));
		}
		if (RandomRotationAtStart)
		{
			_tf.localRotation = Random.rotation;
		}
	}

	private void Update()
	{
		if (!UseInterval)
		{
			return;
		}
		if (RandomChangeIntervalPerAxis)
		{
			_elapsedTimeAxis.x += Time.deltaTime;
			_elapsedTimeAxis.y += Time.deltaTime;
			_elapsedTimeAxis.z += Time.deltaTime;
			if (_elapsedTimeAxis.x > _changeTimeAxis.x)
			{
				_elapsedTimeAxis.x = 0f;
				_speed.x = Random.Range(SpeedMin.x, SpeedMax.x) * (float)((!RandomDirectionPerAxis || Random.Range(0, 2) == 0) ? 1 : (-1));
				_changeTimeAxis.x = Random.Range(ChangeInterval.x, ChangeInterval.y);
			}
			if (_elapsedTimeAxis.y > _changeTimeAxis.y)
			{
				_elapsedTimeAxis.y = 0f;
				_speed.y = Random.Range(SpeedMin.y, SpeedMax.y) * (float)((!RandomDirectionPerAxis || Random.Range(0, 2) == 0) ? 1 : (-1));
				_changeTimeAxis.y = Random.Range(ChangeInterval.x, ChangeInterval.y);
			}
			if (_elapsedTimeAxis.z > _changeTimeAxis.z)
			{
				_elapsedTimeAxis.z = 0f;
				_speed.z = Random.Range(SpeedMin.z, SpeedMax.z) * (float)((!RandomDirectionPerAxis || Random.Range(0, 2) == 0) ? 1 : (-1));
				_changeTimeAxis.z = Random.Range(ChangeInterval.x, ChangeInterval.y);
			}
		}
		else
		{
			_elapsedTime += Time.deltaTime;
			if (_elapsedTime > _changeTime)
			{
				_elapsedTime = 0f;
				_speed.x = Random.Range(SpeedMin.x, SpeedMax.x) * (float)((!RandomDirectionPerAxis || Random.Range(0, 2) == 0) ? 1 : (-1));
				_speed.y = Random.Range(SpeedMin.y, SpeedMax.y) * (float)((!RandomDirectionPerAxis || Random.Range(0, 2) == 0) ? 1 : (-1));
				_speed.z = Random.Range(SpeedMin.z, SpeedMax.z) * (float)((!RandomDirectionPerAxis || Random.Range(0, 2) == 0) ? 1 : (-1));
				_changeTime = Random.Range(ChangeInterval.x, ChangeInterval.y);
			}
		}
		_tf.Rotate(_speed.x * Time.deltaTime, _speed.y * Time.deltaTime, _speed.z * Time.deltaTime);
	}
}
