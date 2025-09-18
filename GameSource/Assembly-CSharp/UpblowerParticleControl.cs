using GameEvent;
using UnityEngine;

public class UpblowerParticleControl : MonoBehaviour, IGameEventListener
{
	protected ParticleSystem.MainModule psm;

	public Transform start;

	public Transform end;

	public float DetectorWidth;

	private Vector2 boxDims;

	protected float distance;

	public float LifeModifier = 1f;

	protected float InitialParticleLifetime;

	public LayerMask layerMask;

	public float ReduceLengthMod = 1f;

	public Gradient runmodeParticlaGradient;

	public Gradient buildModeParticleGradient;

	private ParticleSystem.MinMaxGradient runmodeParticlaGradientB;

	private ParticleSystem.MinMaxGradient buildModeParticleGradientB;

	private Placeable parentPiece;

	private void Start()
	{
		runmodeParticlaGradientB = new ParticleSystem.MinMaxGradient(runmodeParticlaGradient);
		runmodeParticlaGradientB.mode = ParticleSystemGradientMode.RandomColor;
		buildModeParticleGradientB = new ParticleSystem.MinMaxGradient(buildModeParticleGradient);
		buildModeParticleGradientB.mode = ParticleSystemGradientMode.RandomColor;
		ParticleSystem component = GetComponent<ParticleSystem>();
		psm = component.main;
		distance = (end.position - start.position).magnitude;
		ChangeListener(adding: true);
		psm.startColor = buildModeParticleGradient;
		parentPiece = GetComponentInParent<Placeable>();
		boxDims = new Vector2(DetectorWidth, DetectorWidth);
		start.position += start.up * DetectorWidth;
		end.position -= end.up * DetectorWidth;
	}

	public virtual void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<StartPhaseEvent>(this, adding);
	}

	private void Update()
	{
		if (start == null || end == null || (parentPiece != null && parentPiece.MarkedForDestruction))
		{
			psm.startLifetime = 0f;
			return;
		}
		bool queriesHitTriggers = Physics2D.queriesHitTriggers;
		Physics2D.queriesHitTriggers = true;
		float num = Mathf.Atan2(base.transform.right.y, base.transform.right.x);
		float angle = num * 57.29578f;
		RaycastHit2D[] raycastResultCache = Placeable.raycastResultCache;
		int num2 = Physics2D.BoxCastNonAlloc(start.position, boxDims, angle, end.position - start.position, raycastResultCache, distance, layerMask);
		bool flag = false;
		for (int i = 0; i < num2; i++)
		{
			RaycastHit2D raycastHit2D = raycastResultCache[i];
			if (!(raycastHit2D.collider != null))
			{
				continue;
			}
			Placeable componentInParent = raycastHit2D.collider.GetComponentInParent<Placeable>();
			if (!(componentInParent != null) || !(componentInParent == parentPiece))
			{
				CollisionTag component = raycastHit2D.collider.GetComponent<CollisionTag>();
				if (component != null && component.ContainsAnyTag(TagComparer.Tag.Solid))
				{
					psm.startLifetime = Mathf.Min(ReduceLengthMod, raycastHit2D.distance / distance);
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			psm.startLifetimeMultiplier = ReduceLengthMod;
		}
		Physics2D.queriesHitTriggers = queriesHitTriggers;
		psm.startRotationMultiplier = num;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(StartPhaseEvent))
		{
			StartPhaseEvent startPhaseEvent = e as StartPhaseEvent;
			if (startPhaseEvent.Phase == GameControl.GamePhase.PLACE)
			{
				psm.startColor = buildModeParticleGradientB;
			}
			else if (startPhaseEvent.Phase == GameControl.GamePhase.PLAY || startPhaseEvent.Phase == GameControl.GamePhase.SUDDENDEATH)
			{
				psm.startColor = runmodeParticlaGradientB;
			}
		}
	}
}
