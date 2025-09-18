using UnityEngine;

namespace Crosstales.Common.Util;

[DisallowMultipleComponent]
public class RandomScaler : MonoBehaviour
{
	[Tooltip("Use intervals to change the scale (default: true).")]
	public bool UseInterval = true;

	[Tooltip("Random change interval between min (= x) and max (= y) in seconds (default: x = 10, y = 20).")]
	public Vector2 ChangeInterval = new Vector2(10f, 20f);

	[Tooltip("Minimum rotation speed per axis (default: 5 for all axis).")]
	public Vector3 ScaleMin = new Vector3(0.1f, 0.1f, 0.1f);

	[Tooltip("Maximum scale per axis (default: 0.1 for all axis).")]
	public Vector3 ScaleMax = new Vector3(3f, 3f, 3f);

	[Tooltip("Uniform scaling for all axis (x-axis values will be used, default: true).")]
	public bool Uniform = true;

	[Tooltip("Set the object to a random scale at Start (default: false).")]
	public bool RandomScaleAtStart;

	private Transform _tf;

	private Vector3 _startScale;

	private Vector3 _endScale;

	private float _elapsedTime;

	private float _changeTime;

	private float _lerpTime;

	private void Start()
	{
		_tf = base.transform;
		_elapsedTime = (_changeTime = Random.Range(ChangeInterval.x, ChangeInterval.y));
		if (RandomScaleAtStart)
		{
			if (Uniform)
			{
				_startScale.x = (_startScale.y = (_startScale.z = Random.Range(ScaleMin.x, Mathf.Abs(ScaleMax.x))));
			}
			else
			{
				_startScale.x = Random.Range(ScaleMin.x, Mathf.Abs(ScaleMax.x));
				_startScale.y = Random.Range(ScaleMin.y, Mathf.Abs(ScaleMax.y));
				_startScale.z = Random.Range(ScaleMin.z, Mathf.Abs(ScaleMax.z));
			}
			_tf.localScale = _startScale;
		}
		else
		{
			_startScale = _tf.localScale;
		}
	}

	private void Update()
	{
		if (!UseInterval)
		{
			return;
		}
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime > _changeTime)
		{
			_lerpTime = (_elapsedTime = 0f);
			if (Uniform)
			{
				_endScale.x = (_endScale.y = (_endScale.z = Random.Range(ScaleMin.x, Mathf.Abs(ScaleMax.x))));
			}
			else
			{
				_endScale.x = Random.Range(ScaleMin.x, Mathf.Abs(ScaleMax.x));
				_endScale.y = Random.Range(ScaleMin.y, Mathf.Abs(ScaleMax.y));
				_endScale.z = Random.Range(ScaleMin.z, Mathf.Abs(ScaleMax.z));
			}
			_changeTime = Random.Range(ChangeInterval.x, ChangeInterval.y);
		}
		_tf.localScale = Vector3.Lerp(_startScale, _endScale, _lerpTime);
		if (_lerpTime < 1f)
		{
			_lerpTime += Time.deltaTime / (_changeTime - 0.1f);
		}
		else
		{
			_startScale = _tf.localScale;
		}
	}
}
