using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TwitchNameFireworks : MonoBehaviour
{
	private class FireworksTweener
	{
		public Transform transform;

		private Vector3 origin;

		private Vector3 target;

		private float rotateSpeed;

		private AnimationCurve curve;

		private float timer;

		private float duration = 1f;

		public bool done;

		public FireworksTweener(Transform transform, Vector3 localOrigin, Vector3 targetLocalPosition, float rotateSpeed, AnimationCurve curve)
		{
			this.transform = transform;
			origin = localOrigin;
			target = targetLocalPosition;
			this.rotateSpeed = rotateSpeed;
			this.curve = curve;
			duration = curve.keys[curve.length - 1].time;
		}

		public void Update()
		{
			transform.localPosition = Vector3.Lerp(origin, target, curve.Evaluate(timer));
			transform.Rotate(0f, 0f, rotateSpeed);
			if (timer >= duration)
			{
				done = true;
			}
			timer += Time.unscaledDeltaTime;
		}
	}

	public Object twitchFlyingNamePrefab;

	public AnimationCurve fireworksCurve;

	public float fireworksRadiusMin = 200f;

	public float fireworksRadiusMax = 500f;

	public float fireworksMaxRotateSpeed = 5f;

	public float fireworksMinRotateSpeed = -5f;

	public Color[] colors;

	public BoxCollider2D boxCollider;

	private HashSet<FireworksTweener> tweens = new HashSet<FireworksTweener>();

	private int currentNameIdx;

	private string[] names;

	private float timer;

	public void Initialize(IEnumerable<string> names)
	{
		tweens.Clear();
		this.names = names.ToArray();
		timer = 0f;
		currentNameIdx = 0;
	}

	private void Awake()
	{
		VersusControl versusControl = (VersusControl)LobbyManager.instance.CurrentGameController;
		if (versusControl != null)
		{
			versusControl.objectsHoldingUpPlacePhase++;
			versusControl.MainCamera.AddTarget(boxCollider);
		}
	}

	private void OnDestroy()
	{
		VersusControl versusControl = (VersusControl)LobbyManager.instance.CurrentGameController;
		if (versusControl != null)
		{
			versusControl.objectsHoldingUpPlacePhase--;
			versusControl.MainCamera.RemoveTarget(boxCollider);
		}
	}

	private void Update()
	{
		if (names == null || names.Length == 0)
		{
			return;
		}
		float num = Mathf.Min(5f / (float)names.Length, 0.3f);
		timer += Time.unscaledDeltaTime;
		while (currentNameIdx < names.Length && timer > num)
		{
			Text text = base.gameObject.AddPrefabAsChild<Text>(twitchFlyingNamePrefab);
			text.text = names[currentNameIdx];
			AkSoundEngine.PostEvent("UI_Twitch_Courtesy_of_Firework", base.gameObject);
			if (colors != null)
			{
				text.color = colors[Random.Range(0, colors.Length)];
			}
			Vector3 vector = Quaternion.Euler(0f, 0f, Random.Range(-90f, 90f)) * new Vector3(0f, Random.Range(fireworksRadiusMin, fireworksRadiusMax), 0f);
			Vector3 localOrigin = vector * 0.01f;
			float rotateSpeed = Random.Range(fireworksMinRotateSpeed, fireworksMaxRotateSpeed);
			tweens.Add(new FireworksTweener(text.transform, localOrigin, vector, rotateSpeed, fireworksCurve));
			currentNameIdx++;
			timer -= num;
		}
		HashSet<FireworksTweener> hashSet = new HashSet<FireworksTweener>();
		foreach (FireworksTweener tween in tweens)
		{
			tween.Update();
			if (tween.done)
			{
				hashSet.Add(tween);
			}
		}
		foreach (FireworksTweener item in hashSet)
		{
			Object.Destroy(item.transform.gameObject);
			tweens.Remove(item);
		}
		if (tweens.Count == 0 && currentNameIdx >= names.Length)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
