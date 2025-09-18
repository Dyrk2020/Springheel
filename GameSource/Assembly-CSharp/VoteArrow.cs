using UnityEngine;

public class VoteArrow : MonoBehaviour
{
	public enum LightState
	{
		OFF,
		SOLID,
		FLASHING
	}

	protected Animator animator;

	protected Animator lightAnimator;

	public bool VoteLocked;

	protected bool chrPresent;

	protected bool buttonPressed;

	protected bool tempDisabled;

	public LightState lightState;

	public Character lastCharacterSelected;

	public SpriteRenderer ArrowSR;

	public SpriteRenderer DarkLightsSR;

	public LevelPortal levelPortal;

	public SpriteRenderer[] srs;

	public bool ChrPresent => chrPresent;

	public bool ButtonPressed
	{
		get
		{
			return buttonPressed;
		}
		set
		{
			buttonPressed = value;
		}
	}

	public bool TempDisabled
	{
		get
		{
			return tempDisabled;
		}
		set
		{
			tempDisabled = value;
		}
	}

	private void Start()
	{
		animator = GetComponent<Animator>();
		lightAnimator = base.transform.GetChild(0).GetComponentInChildren<Animator>();
		characterLeft();
		lightAnimator.SetInteger("LightState", (int)lightState);
		srs = GetComponentsInChildren<SpriteRenderer>();
	}

	private void Update()
	{
		if ((chrPresent && buttonPressed && !tempDisabled && lastCharacterSelected.OnGround) || VoteLocked)
		{
			if (!animator.enabled)
			{
				animator.enabled = true;
			}
			if (!lightAnimator.enabled)
			{
				lightAnimator.enabled = true;
			}
			animator.SetBool("Enabled", value: true);
		}
		else
		{
			animator.SetBool("Enabled", value: false);
		}
		if (lightAnimator.enabled)
		{
			lightAnimator.SetInteger("LightState", (int)lightState);
		}
	}

	public void characterPresent()
	{
		chrPresent = true;
		tempDisabled = false;
	}

	public void characterLeft()
	{
		chrPresent = false;
		if (!VoteLocked)
		{
			lightState = LightState.OFF;
		}
	}

	public void enableSpriteRenders()
	{
		enableSpriteRenders(show: true);
	}

	public void enableSpriteRenders(bool show)
	{
		SpriteRenderer[] array = srs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = show;
		}
	}

	public void disableSpriteRenderers()
	{
		enableSpriteRenders(show: false);
		DisableAnimator();
	}

	public void setColor(Color arrowColor, Color darkLights)
	{
		ArrowSR.color = arrowColor;
		DarkLightsSR.color = darkLights;
	}

	public void DisableAnimator()
	{
		animator.enabled = false;
		lightAnimator.enabled = false;
	}
}
