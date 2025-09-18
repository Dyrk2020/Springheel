using UnityEngine;

public class Spectator : MonoBehaviour
{
	public enum SpectatorState
	{
		IDLE,
		VICTORY,
		DYING,
		DEAD
	}

	private Animator anim;

	private SpriteRenderer spriteR;

	protected SpectatorState state;

	public SpriteRenderer SpriteRender => spriteR;

	private void Awake()
	{
		anim = GetComponent<Animator>();
		spriteR = GetComponent<SpriteRenderer>();
		Hide();
	}

	public SpectatorState GetState()
	{
		return state;
	}

	public void SetState(SpectatorState state)
	{
		this.state = state;
		anim.SetInteger("State", (int)state);
	}

	public void Show()
	{
		spriteR.enabled = true;
		anim.enabled = true;
	}

	public void Hide()
	{
		spriteR.enabled = false;
		anim.enabled = false;
	}
}
