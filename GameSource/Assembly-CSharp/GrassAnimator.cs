using System.Collections;
using UnityEngine;

public class GrassAnimator : MonoBehaviour
{
	public float Framerate = 12f;

	public Sprite[] grassSpriteAnimation;

	public SpriteRenderer[] grassSR;

	private int[] grassNum;

	private Coroutine updateGrassCoroutine;

	private WaitForSeconds waiter;

	public bool loop;

	private bool initialized;

	public void Start()
	{
		grassNum = new int[grassSR.Length];
		for (int i = 0; i < grassNum.Length; i++)
		{
			grassNum[i] = Random.Range(-grassSpriteAnimation.Length + 1, grassSpriteAnimation.Length - 1);
		}
		waiter = new WaitForSeconds(1f / Framerate);
		initialized = true;
	}

	public void OnEnable()
	{
		updateGrassCoroutine = StartCoroutine(UpdateGrass());
	}

	private IEnumerator UpdateGrass()
	{
		while (!initialized)
		{
			yield return null;
		}
		while (true)
		{
			for (int i = 0; i < grassSR.Length; i++)
			{
				grassNum[i]++;
				if (grassNum[i] >= grassSpriteAnimation.Length)
				{
					if (loop)
					{
						grassNum[i] = 0;
					}
					else
					{
						grassNum[i] = -grassSpriteAnimation.Length + 2;
					}
				}
				grassSR[i].sprite = grassSpriteAnimation[Mathf.Abs(grassNum[i])];
			}
			yield return waiter;
		}
	}

	private void OnDestroy()
	{
		if (updateGrassCoroutine != null)
		{
			StopCoroutine(updateGrassCoroutine);
		}
	}
}
