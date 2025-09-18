using UnityEngine;

public class TimeController : MonoBehaviour
{
	private float lastTimeScale;

	private bool timeStopped;

	private void Start()
	{
	}

	private void Update()
	{
		if (!GameState.DebugMode || Input.GetKey(KeyCode.LeftShift))
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			if (timeStopped)
			{
				Time.timeScale = lastTimeScale;
				timeStopped = false;
			}
			else
			{
				timeStopped = true;
				lastTimeScale = Time.timeScale;
				Time.timeScale = 0f;
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			Time.timeScale = 0.03f;
			timeStopped = false;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			Time.timeScale = 0.1f;
			timeStopped = false;
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			Time.timeScale = 0.3f;
			timeStopped = false;
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			Time.timeScale = 0.5f;
			timeStopped = false;
		}
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			Time.timeScale = 1f;
			timeStopped = false;
		}
		if (Input.GetKeyDown(KeyCode.Equals))
		{
			Time.timeScale = 2f;
			timeStopped = false;
		}
	}
}
