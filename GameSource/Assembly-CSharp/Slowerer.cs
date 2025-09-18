using System.Threading;
using UnityEngine;

public class Slowerer : MonoBehaviour
{
	public static Slowerer instance;

	public bool delayUpdate;

	public bool delayFixedUpdate;

	public int minimumDelayMilliseconds = 20;

	private void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (delayUpdate)
		{
			Thread.Sleep(minimumDelayMilliseconds);
		}
	}

	private void FixedUpdate()
	{
		if (delayFixedUpdate)
		{
			Thread.Sleep(minimumDelayMilliseconds);
		}
	}
}
