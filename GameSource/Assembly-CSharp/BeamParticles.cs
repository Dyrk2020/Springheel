using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BeamParticles : MonoBehaviour
{
	private ParticleSystem particles;

	private Transform target;

	private Character emitter;

	private GameSettings.animalColors colours;

	public AnimationCurve BeamAlphaVsDistance;

	public AnimationCurve BeamGrabVsDistance;

	public float MinDistance;

	public float MaxDistance;

	public AnimationCurve AlphaFromAngle;

	public float MinAngle;

	public float MaxAngle;

	public Gradient DefaultColours;

	public bool UseCharacterColours;

	public float MinParticleSpeed;

	public float MaxParticleSpeed;

	public float MinEmissionRate;

	public float MaxEmissionRate;

	public LineRenderer lineRenderer;

	public List<Vector3> linePoints;

	public Color lineColour;

	public float BaseBeamAlpha;

	private void Awake()
	{
		particles = GetComponent<ParticleSystem>();
	}

	private void Start()
	{
		stopBeam();
	}

	private void startBeam()
	{
		if (!particles.isPlaying)
		{
			particles.Play();
		}
		if (linePoints.Count == 0)
		{
			linePoints.Add(default(Vector3));
			linePoints.Add(default(Vector3));
		}
		lineRenderer.enabled = true;
	}

	private void stopBeam()
	{
		particles.Stop();
		lineRenderer.enabled = false;
	}

	private void Update()
	{
		if (emitter == null || !emitter.Enabled)
		{
			stopBeam();
		}
		else if (target != null)
		{
			ParticleSystem.MainModule main = particles.main;
			ParticleSystem.ShapeModule shape = particles.shape;
			ParticleSystem.EmissionModule emission = particles.emission;
			Vector3 vector = target.position - base.transform.position;
			if (vector.sqrMagnitude > MaxDistance * MaxDistance)
			{
				stopBeam();
			}
			else
			{
				startBeam();
				float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				float time = Mathf.Clamp01((vector.magnitude - MinDistance) / (MaxDistance - MinDistance));
				float time2 = Mathf.Clamp01((num - MinAngle) / (MaxAngle - MinAngle));
				float num2 = BeamAlphaVsDistance.Evaluate(time);
				float num3 = AlphaFromAngle.Evaluate(time2);
				float t = BeamGrabVsDistance.Evaluate(time);
				ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
				startSpeed.constant = num2 * (MaxParticleSpeed - MinParticleSpeed) + MinParticleSpeed;
				startSpeed.mode = ParticleSystemCurveMode.Constant;
				ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
				rateOverTime.constant = num2 * num3 * (MaxEmissionRate - MinEmissionRate) + MinEmissionRate;
				rateOverTime.mode = ParticleSystemCurveMode.Constant;
				emission.rateOverTime = rateOverTime;
				shape.rotation = new Vector3(0f, 0f, num - 90f);
				main.startSpeed = startSpeed.constant;
				linePoints[0] = Vector3.Lerp(target.position, emitter.transform.position, t);
				linePoints[1] = target.position;
				lineRenderer.startColor = new Color(lineColour.r, lineColour.g, lineColour.b, BaseBeamAlpha * num2 * num3);
				lineRenderer.endColor = new Color(lineColour.r, lineColour.g, lineColour.b, BaseBeamAlpha * num2 * num3);
			}
		}
		DrawLine();
	}

	public void SetCharacter(Character character)
	{
		emitter = character;
		colours = GameSettings.GetInstance().characterColors[(int)(character.CharacterSprite - 1)];
		ParticleSystem.MainModule main = particles.main;
		ParticleSystem.MinMaxGradient startColor = main.startColor;
		startColor.mode = ParticleSystemGradientMode.RandomColor;
		GradientColorKey[] colorKeys = new GradientColorKey[2]
		{
			new GradientColorKey(colours.mainColor, 0.8f),
			new GradientColorKey(colours.mainColor, 1f)
		};
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		};
		startColor.gradient.SetKeys(colorKeys, alphaKeys);
		startColor.gradient.mode = GradientMode.Fixed;
		main.startColor = startColor;
		lineRenderer.startColor = colours.mainColor;
		lineRenderer.endColor = colours.mainColor;
		lineColour = colours.mainColor;
	}

	public void SetTarget(Transform target)
	{
		this.target = target;
	}

	private void DrawLine()
	{
		lineRenderer.positionCount = linePoints.Count;
		for (int i = 0; i < linePoints.Count; i++)
		{
			lineRenderer.SetPosition(i, linePoints[i]);
		}
	}
}
