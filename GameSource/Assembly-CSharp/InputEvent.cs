public class InputEvent
{
	public enum InputKey
	{
		Up,
		Down,
		Left,
		Right,
		OrthoUp,
		OrthoDown,
		OrthoLeft,
		OrthoRight,
		Jump,
		Suicide,
		ChangeMode,
		Sprint,
		Inventory,
		Zoom,
		LeftTrigger,
		RightTrigger,
		RotateLeft,
		RotateRight,
		Start,
		Pause,
		Scoreboard,
		Accept,
		Back,
		NoKey,
		Up2,
		Down2,
		Left2,
		Right2,
		OrthoUp2,
		OrthoDown2,
		OrthoLeft2,
		OrthoRight2,
		VectorChanged,
		Chat,
		Esc,
		DpadUp,
		DpadDown,
		DpadLeft,
		DpadRight
	}

	public Controller Sender;

	private bool consumed;

	public int PlayerBitMask { get; protected set; }

	public InputKey Key { get; protected set; }

	public float Valuef { get; protected set; }

	public bool Valueb { get; protected set; }

	public bool Changed { get; protected set; }

	public bool Consumed => consumed;

	public InputEvent(int player, InputKey key, bool valueb, bool changed)
	{
		PlayerBitMask = player;
		Key = key;
		Valuef = (valueb ? 1 : 0);
		Valueb = valueb;
		Changed = changed;
	}

	public InputEvent(int player, InputKey key, float valuef, bool changed)
	{
		PlayerBitMask = player;
		Key = key;
		Valuef = valuef;
		Valueb = valuef != 0f;
		Changed = changed;
	}

	public InputEvent(int player, InputKey key, float valuef, bool valueb, bool changed)
	{
		PlayerBitMask = player;
		Key = key;
		Valuef = valuef;
		Valueb = valueb;
		Changed = changed;
	}

	public InputEvent()
	{
		consumed = true;
		Key = InputKey.NoKey;
	}

	public void Reset(int player, InputKey key, float valuef, bool valueb, bool changed)
	{
		PlayerBitMask = player;
		Key = key;
		Valuef = valuef;
		Valueb = valueb;
		Changed = changed;
		consumed = false;
	}

	public void Reset(int player, InputKey key, float valuef, bool changed)
	{
		PlayerBitMask = player;
		Key = key;
		Valuef = valuef;
		Valueb = valuef != 0f;
		Changed = changed;
		consumed = false;
	}

	public void Reset(int player, InputKey key, bool valueb, bool changed)
	{
		PlayerBitMask = player;
		Key = key;
		Valuef = (valueb ? 1 : 0);
		Valueb = valueb;
		Changed = changed;
		consumed = false;
	}

	public void Consume()
	{
		consumed = true;
	}

	public override string ToString()
	{
		return "(Player " + PlayerBitMask + ") " + Key.ToString() + ": " + Valuef + " (" + Valueb + ") " + (Changed ? "!" : "");
	}
}
