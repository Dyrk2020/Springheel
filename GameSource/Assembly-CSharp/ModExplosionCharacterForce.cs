using GameEvent;
using UnityEngine;

public class ModExplosionCharacterForce : MonoBehaviour, IGameEventListener
{
	public Transform CenterOfMass;

	protected Vector3 previousCenterOfMass;

	protected Vector3 velocity;

	public float Strength;

	public float extraUp;

	public float CenterStrengthMultiplier;

	public float EdgeStrengthModifier;

	protected Animator animator;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Character componentInParent = collision.gameObject.GetComponentInParent<Character>();
		if (!(componentInParent == null))
		{
			Modifiers instance = Modifiers.GetInstance();
			Vector2 vector = componentInParent.transform.position - CenterOfMass.position;
			float num = Mathf.Lerp(CenterStrengthMultiplier, EdgeStrengthModifier, vector.magnitude / instance.ProjectileExplosionScale);
			componentInParent.AddImpulse(vector.normalized * instance.ProjectileExplosionScale * Strength * num + Vector2.up * extraUp, 0.1f);
		}
	}

	private void Awake()
	{
		ChangeListener(adding: true);
		animator = GetComponent<Animator>();
	}

	public virtual void OnDestroy()
	{
		ChangeListener(adding: false);
	}

	public void ChangeListener(bool adding)
	{
		GameEventManager.ChangeListener<PauseEvent>(this, adding);
	}

	private void Pause()
	{
		animator.speed = 0f;
	}

	private void Unpause()
	{
		animator.speed = 1f;
	}

	public void handleEvent(global::GameEvent.GameEvent e)
	{
		if (e.GetType() == typeof(PauseEvent))
		{
			if ((e as PauseEvent).Paused)
			{
				Pause();
			}
			else
			{
				Unpause();
			}
		}
	}
}
