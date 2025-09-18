using UnityEngine;
using UnityEngine.Networking;

public class mouseTest : Cursor
{
	protected bool Hovering;

	public override void Start()
	{
		GameState.GetInstance();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (hoveredPick != null)
		{
			Hovering = true;
			if (lastHoveredPick != hoveredPick)
			{
				lastHoveredPick = hoveredPick;
				lastHoveredPick.PlayHoverSound();
			}
			hoveredPick.IHoveredCursors.Remove(this);
			hoveredPick = null;
		}
		else
		{
			Hovering = false;
		}
	}

	protected override void Update()
	{
		if (Input.GetMouseButtonDown(0) && lastHoveredPick != null && Hovering)
		{
			Debug.Log("You cliked " + lastHoveredPick.Name);
		}
		base.transform.position = Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(10f);
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
	}

	private void OnTriggerStay2D(Collider2D c)
	{
	}

	protected override void OnAccept()
	{
		base.OnAccept();
		if (lastHoveredPick != null)
		{
			Debug.Log("You cliked " + lastHoveredPick.Name);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
