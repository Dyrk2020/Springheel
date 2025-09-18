using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PartyCursor : PiecePlacementCursor
{
	public bool Picked;

	private bool lockSet;

	private static bool placementLock;

	protected override void SetSprites(CharacterSpriteLibrary spriteLib)
	{
		base.SetSprites(spriteLib);
		((BoxCollider2D)SelectionCollider).offset = ((BoxCollider2D)SelectionCollider).offset + new Vector2((0f - PieceOffset.x) / 2f, (0f - PieceOffset.y) / 2f);
		bad = ok;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (hoveredPiece != null && !hoveredPiece.Enabled)
		{
			hoveredPiece = null;
		}
		if (lockSet)
		{
			placementLock = false;
			lockSet = false;
		}
	}

	public override void SetPiece(Placeable piece, bool destroyPrevious, bool pickup)
	{
		base.SetPiece(piece, destroyPrevious, pickup);
		if (piece != null)
		{
			SelectionCollider.enabled = false;
		}
		else
		{
			SelectionCollider.enabled = true;
		}
	}

	protected override void OnAccept()
	{
		if (acceptDown)
		{
			StartCoroutine(tryPickPiece());
		}
	}

	protected override void OnBack()
	{
		base.OnBack();
	}

	public override void Enable()
	{
		base.Enable();
		Picked = false;
		SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.UIPOOF, base.transform.position - new Vector3(0.5f, 0.5f, 0f), 0.7f);
	}

	private IEnumerator tryPickPiece()
	{
		while (placementLock)
		{
			yield return null;
		}
		placementLock = true;
		lockSet = true;
		if (base.Piece == null && hoveredPiece != null && !hoveredPiece.PickedUp)
		{
			SetPiece(hoveredPiece, destroyPrevious: true, pickup: true);
			hoveredPiece.PickedUp = true;
			hoveredPiece.HoveredCursors.Clear();
			hoveredPiece = null;
			if (base.Piece != null)
			{
				SmokePool.Instance.SpawnSmoke(SmokePool.SmokeType.UIPOOF, base.transform.position, 0.7f);
				AkSoundEngine.PostEvent("UI_Inventory_Select_" + base.Piece.SFXEventName, base.gameObject);
				AkSoundEngine.PostEvent("UI_InGame_PartyBox_Select_Item", base.gameObject);
				Freeze();
				Disable();
			}
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
