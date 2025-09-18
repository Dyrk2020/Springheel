using UnityEngine;

namespace Dreamteck.Splines;

[ExecuteInEditMode]
[AddComponentMenu("Dreamteck/Splines/Users/Particle Controller")]
public class ParticleController : SplineUser
{
	public enum EmitPoint
	{
		Beginning,
		Ending,
		Random,
		Ordered
	}

	public enum MotionType
	{
		None,
		UseParticleSystem,
		FollowForward,
		FollowBackward,
		ByNormal,
		ByNormalRandomized
	}

	public enum Wrap
	{
		Default,
		Loop
	}

	public class Particle
	{
		internal Vector2 startOffset = Vector2.zero;

		internal Vector2 endOffset = Vector2.zero;

		internal float cycleSpeed;

		internal float startLifetime;

		internal Color startColor = Color.white;

		internal float remainingLifetime;

		internal float lifeTime;

		internal double startPercent;

		internal double GetSplinePercent(Wrap wrap, ParticleSystem.Particle particle, MotionType motionType)
		{
			float num = particle.remainingLifetime / particle.startLifetime;
			if (motionType == MotionType.FollowBackward)
			{
				num = 1f - num;
			}
			switch (wrap)
			{
			case Wrap.Default:
				return DMath.Clamp01(startPercent + (double)((1f - num) * cycleSpeed));
			case Wrap.Loop:
			{
				double num2 = startPercent + (1.0 - (double)num) * (double)cycleSpeed;
				if (num2 > 1.0)
				{
					num2 -= (double)Mathf.FloorToInt((float)num2);
				}
				return num2;
			}
			default:
				return 0.0;
			}
		}
	}

	[HideInInspector]
	public ParticleSystem _particleSystem;

	[HideInInspector]
	public bool volumetric;

	[HideInInspector]
	public bool emitFromShell;

	[HideInInspector]
	public Vector2 scale = Vector2.one;

	[HideInInspector]
	public EmitPoint emitPoint;

	[HideInInspector]
	public MotionType motionType = MotionType.UseParticleSystem;

	[HideInInspector]
	public Wrap wrapMode;

	[HideInInspector]
	public float minCycles = 1f;

	[HideInInspector]
	public float maxCycles = 1f;

	private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[0];

	private Particle[] controllers = new Particle[0];

	private int particleCount;

	private int birthIndex;

	protected override void LateRun()
	{
		if (_particleSystem == null)
		{
			return;
		}
		int maxParticles = _particleSystem.main.maxParticles;
		if (particles.Length != maxParticles)
		{
			particles = new ParticleSystem.Particle[maxParticles];
			Particle[] array = new Particle[maxParticles];
			for (int i = 0; i < array.Length && i < controllers.Length; i++)
			{
				array[i] = controllers[i];
			}
			controllers = array;
		}
		particleCount = _particleSystem.GetParticles(particles);
		bool flag = _particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local;
		Transform transform = _particleSystem.transform;
		for (int num = particleCount - 1; num >= 0; num--)
		{
			if (flag)
			{
				TransformParticle(ref particles[num], transform);
			}
			if (controllers[num] == null)
			{
				controllers[num] = new Particle();
				OnParticleBorn(num);
				if (flag)
				{
					InverseTransformParticle(ref particles[num], transform);
				}
			}
			else
			{
				float num2 = particles[num].startLifetime - particles[num].remainingLifetime;
				if (num2 <= Time.deltaTime && controllers[num].lifeTime > num2)
				{
					OnParticleBorn(num);
				}
				if (flag)
				{
					InverseTransformParticle(ref particles[num], transform);
				}
			}
		}
		for (int j = 0; j < particleCount; j++)
		{
			if (controllers[j] == null)
			{
				controllers[j] = new Particle();
			}
			if (flag)
			{
				TransformParticle(ref particles[j], transform);
			}
			HandleParticle(j);
			if (flag)
			{
				InverseTransformParticle(ref particles[j], transform);
			}
		}
		_particleSystem.SetParticles(particles, particleCount);
	}

	private void TransformParticle(ref ParticleSystem.Particle particle, Transform trs)
	{
		particle.position = trs.TransformPoint(particle.position);
		particle.velocity = trs.TransformDirection(particle.velocity);
	}

	private void InverseTransformParticle(ref ParticleSystem.Particle particle, Transform trs)
	{
		particle.position = trs.InverseTransformPoint(particle.position);
		particle.velocity = trs.InverseTransformDirection(particle.velocity);
	}

	protected override void Reset()
	{
		base.Reset();
		updateMethod = UpdateMethod.LateUpdate;
		if (_particleSystem == null)
		{
			_particleSystem = GetComponent<ParticleSystem>();
		}
	}

	private void HandleParticle(int index)
	{
		float num = particles[index].remainingLifetime / particles[index].startLifetime;
		if (motionType == MotionType.FollowBackward || motionType == MotionType.FollowForward || motionType == MotionType.None)
		{
			Evaluate(controllers[index].GetSplinePercent(wrapMode, particles[index], motionType), evalResult);
			ModifySample(evalResult);
			particles[index].position = evalResult.position;
			if (volumetric)
			{
				Vector3 vector = -Vector3.Cross(evalResult.forward, evalResult.up);
				Vector2 vector2 = controllers[index].startOffset;
				if (motionType != MotionType.None)
				{
					vector2 = Vector2.Lerp(controllers[index].startOffset, controllers[index].endOffset, 1f - num);
				}
				particles[index].position += vector * vector2.x * scale.x * evalResult.size + evalResult.up * vector2.y * scale.y * evalResult.size;
			}
			particles[index].velocity = evalResult.forward;
			particles[index].startColor = controllers[index].startColor * evalResult.color;
		}
		controllers[index].remainingLifetime = particles[index].remainingLifetime;
		controllers[index].lifeTime = particles[index].startLifetime - particles[index].remainingLifetime;
	}

	private void OnParticleDie(int index)
	{
	}

	private void OnParticleBorn(int index)
	{
		birthIndex++;
		double num = 0.0;
		float num2 = Mathf.Lerp(_particleSystem.emission.rateOverTime.constantMin, _particleSystem.emission.rateOverTime.constantMax, 0.5f) * _particleSystem.main.startLifetime.constantMax;
		if ((float)birthIndex > num2)
		{
			birthIndex = 0;
		}
		switch (emitPoint)
		{
		case EmitPoint.Beginning:
			num = 0.0;
			break;
		case EmitPoint.Ending:
			num = 1.0;
			break;
		case EmitPoint.Random:
			num = Random.Range(0f, 1f);
			break;
		case EmitPoint.Ordered:
			num = ((num2 > 0f) ? ((float)birthIndex / num2) : 0f);
			break;
		}
		Evaluate(num, evalResult);
		ModifySample(evalResult);
		controllers[index].startColor = particles[index].startColor;
		controllers[index].startPercent = num;
		controllers[index].startLifetime = particles[index].startLifetime;
		controllers[index].remainingLifetime = particles[index].remainingLifetime;
		controllers[index].cycleSpeed = Random.Range(minCycles, maxCycles);
		Vector2 vector = Vector2.zero;
		if (volumetric)
		{
			vector = ((!emitFromShell) ? Random.insideUnitCircle : ((Vector2)(Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Vector2.right)));
		}
		controllers[index].startOffset = vector * 0.5f;
		controllers[index].endOffset = Random.insideUnitCircle * 0.5f;
		Vector3 vector2 = Vector3.Cross(evalResult.forward, evalResult.up);
		particles[index].position = evalResult.position + vector2 * controllers[index].startOffset.x * evalResult.size * scale.x + evalResult.up * controllers[index].startOffset.y * evalResult.size * scale.y;
		float x = _particleSystem.forceOverLifetime.x.constantMax;
		float y = _particleSystem.forceOverLifetime.y.constantMax;
		float z = _particleSystem.forceOverLifetime.z.constantMax;
		if (_particleSystem.forceOverLifetime.randomized)
		{
			x = Random.Range(_particleSystem.forceOverLifetime.x.constantMin, _particleSystem.forceOverLifetime.x.constantMax);
			y = Random.Range(_particleSystem.forceOverLifetime.y.constantMin, _particleSystem.forceOverLifetime.y.constantMax);
			z = Random.Range(_particleSystem.forceOverLifetime.z.constantMin, _particleSystem.forceOverLifetime.z.constantMax);
		}
		float num3 = particles[index].startLifetime - particles[index].remainingLifetime;
		Vector3 vector3 = new Vector3(x, y, z) * 0.5f * (num3 * num3);
		float constantMax = _particleSystem.main.startSpeed.constantMax;
		if (motionType == MotionType.ByNormal)
		{
			particles[index].position += evalResult.up * constantMax * (particles[index].startLifetime - particles[index].remainingLifetime);
			particles[index].position += vector3;
			particles[index].velocity = evalResult.up * constantMax + new Vector3(x, y, z) * num3;
		}
		else if (motionType == MotionType.ByNormalRandomized)
		{
			Vector3 vector4 = Quaternion.AngleAxis(Random.Range(0f, 360f), evalResult.forward) * evalResult.up;
			particles[index].position += vector4 * constantMax * (particles[index].startLifetime - particles[index].remainingLifetime);
			particles[index].position += vector3;
			particles[index].velocity = vector4 * constantMax + new Vector3(x, y, z) * num3;
		}
		HandleParticle(index);
	}
}
