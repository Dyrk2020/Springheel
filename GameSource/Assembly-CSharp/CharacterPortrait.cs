using UnityEngine;

public class CharacterPortrait : MonoBehaviour
{
	public Sprite[] Icons;

	public Sprite[] Phases;

	public SpriteRenderer Icon;

	public SpriteRenderer Phase;

	public SpriteRenderer Name;

	public SpriteRenderer Crown;

	public int CurrentPhase;

	private bool winning;

	public bool Winning
	{
		get
		{
			return winning;
		}
		set
		{
			winning = value;
			Crown.enabled = winning;
		}
	}

	private void Start()
	{
		Winning = false;
	}

	private void Update()
	{
	}

	public void SwapPhase()
	{
		CurrentPhase = ((CurrentPhase > 1) ? 1 : ((CurrentPhase >= 0) ? CurrentPhase : 0));
		CurrentPhase ^= 1;
		Phase.sprite = Phases[CurrentPhase];
	}
}
