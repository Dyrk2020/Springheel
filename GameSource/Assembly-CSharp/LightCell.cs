using System;
using System.Collections;
using UnityEngine;

public class LightCell : MonoBehaviour
{
	public enum State
	{
		OFF,
		LIT,
		SOLID
	}

	[Flags]
	public enum Neighbour
	{
		TOP = 1,
		RIGHT = 2,
		BOTTOM = 4,
		LEFT = 8
	}

	private const Neighbour AllNeighbours = Neighbour.TOP | Neighbour.RIGHT | Neighbour.BOTTOM | Neighbour.LEFT;

	public State CellState;

	public float ColourFadeTime = 0.25f;

	public float triggerRadius;

	public Color DefaultColour = Color.cyan;

	public float CycleOffset;

	public Sprite[] FilledSprites;

	public Sprite[] EmptySprites;

	[HideInInspector]
	public int xPos;

	[HideInInspector]
	public int yPos;

	[HideInInspector]
	public LightGrid grid;

	public float FadeTime;

	public AnimationCurve FadeCurve;

	private bool fading;

	public float PulsePeriod;

	public AnimationCurve LitPulseCurve;

	public AnimationCurve UnlitPulseCurve;

	private bool pulsing;

	private SpriteRenderer spriteRenderer;

	private BoxCollider2D boxCollider;

	private float colourLerp;

	private float lerpTarget = 1f;

	private int neighbours;

	private float animTime;

	private float animLength;

	public CollisionTag collisionTag;

	[BitMask(typeof(TagComparer.Tag))]
	public TagComparer.Tag solidObjectTag = (TagComparer.Tag)263296;

	[BitMask(typeof(TagComparer.Tag))]
	public TagComparer.Tag nonSolidObjectTag = TagComparer.Tag.NoAttach;

	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		boxCollider = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		spriteRenderer.color = DefaultColour;
		spriteRenderer.sprite = EmptySprites[0];
	}

	private void Update()
	{
		pulseCell();
	}

	private void pulseCell()
	{
		if (Time.timeSinceLevelLoad < CycleOffset)
		{
			return;
		}
		Color color = spriteRenderer.color;
		if (!base.enabled)
		{
			return;
		}
		animLength = PulsePeriod;
		if (CellState != State.OFF && !fading)
		{
			pulsing = true;
		}
		if (animTime < animLength)
		{
			animTime += Time.deltaTime;
			if (pulsing)
			{
				color = spriteRenderer.color;
				if (CellState == State.LIT)
				{
					color.a = UnlitPulseCurve.Evaluate(animTime / PulsePeriod);
				}
				else if (CellState == State.SOLID)
				{
					color.a = LitPulseCurve.Evaluate(animTime / PulsePeriod);
				}
				spriteRenderer.color = color;
			}
		}
		else
		{
			pulsing = false;
			animTime -= animLength;
		}
	}

	private IEnumerator AnimateStateTransition(bool reverse = false)
	{
		while (pulsing || fading)
		{
			yield return null;
		}
		fading = true;
		float animTime = 0f;
		_ = spriteRenderer.color;
		while (animTime < FadeTime)
		{
			animTime += Time.deltaTime;
			float num = animTime / FadeTime;
			if (num > 1f)
			{
				num = 1f;
			}
			if (reverse)
			{
				num = 1f - num;
			}
			Color color = spriteRenderer.color;
			color.a = FadeCurve.Evaluate(num);
			spriteRenderer.color = color;
			yield return null;
		}
		fading = false;
	}

	public void TurnOn(Color color)
	{
		spriteRenderer.color = color;
		CellState = State.LIT;
		lerpTarget = 1f;
		setSprite();
		AkSoundEngine.PostEvent("SFX_Level_Light_Light_On", base.gameObject);
	}

	public void Solidify()
	{
		StartCoroutine(AnimateStateTransition());
		CellState = State.SOLID;
		setSprite();
		boxCollider.enabled = true;
	}

	public void TurnOff()
	{
		StartCoroutine(AnimateStateTransition(reverse: true));
		CellState = State.OFF;
		lerpTarget = 0f;
		boxCollider.enabled = false;
		setSprite();
	}

	public void SetNeighbour(Neighbour neighbour, bool lit)
	{
		if (lit)
		{
			neighbours |= (int)neighbour;
		}
		else
		{
			neighbours &= (int)((Neighbour.TOP | Neighbour.RIGHT | Neighbour.BOTTOM | Neighbour.LEFT) ^ neighbour);
		}
		setSprite();
	}

	private void setSprite()
	{
		switch (CellState)
		{
		case State.OFF:
			spriteRenderer.sprite = EmptySprites[0];
			break;
		case State.LIT:
			spriteRenderer.sprite = EmptySprites[1];
			break;
		case State.SOLID:
			spriteRenderer.sprite = FilledSprites[0];
			break;
		}
	}

	private IEnumerator fadeToColour(Color color, float time)
	{
		bool doneLerping = false;
		if (!(colourLerp < lerpTarget))
		{
		}
		while (!doneLerping)
		{
			float num = ((colourLerp < lerpTarget) ? 1 : (-1));
			colourLerp += num * Time.deltaTime / time;
			if (colourLerp < 0f)
			{
				colourLerp = 0f;
				doneLerping = true;
			}
			else if (colourLerp > 1f)
			{
				colourLerp = 1f;
				doneLerping = true;
			}
			spriteRenderer.color = Color.Lerp(DefaultColour, color, colourLerp);
			yield return null;
		}
		if (lerpTarget == 0f)
		{
			spriteRenderer.color = DefaultColour;
		}
		else
		{
			spriteRenderer.color = color;
		}
	}
}
