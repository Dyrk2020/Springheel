using System;
using System.Collections.Generic;
using UnityEngine;

public class GroupDebugPrint : MonoBehaviour
{
	private void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.F2))
		{
			return;
		}
		List<Placeable> list = new List<Placeable>();
		HashSet<AttachmentGroup> hashSet = new HashSet<AttachmentGroup>();
		Placeable[] array = UnityEngine.Object.FindObjectsOfType<Placeable>();
		foreach (Placeable placeable in array)
		{
			if (placeable.Group != null)
			{
				list.Add(placeable);
				hashSet.Add(placeable.Group);
			}
		}
		string text = "Dumping groups... (Total: " + hashSet.Count + ")\n";
		text += "\nPlaceables with groups:\n";
		foreach (Placeable item in list)
		{
			text = text + "\t" + item.UsefulName + ": " + item.Group.id + "\n";
		}
		text += "\n";
		foreach (AttachmentGroup item2 in hashSet)
		{
			HashSet<AttachmentNode> hashSet2 = new HashSet<AttachmentNode>();
			if (item2.TopParent != null)
			{
				text = text + "Group " + item2.id + " of [" + item2.TopParent.ID + (item2.TopParent.isSetPiece ? "S" : "") + "] " + item2.TopParent.name + "\n";
				if (item2.TopParent.Group != item2)
				{
					text = text + "WARNING: Group " + item2.id + "'s top parent is not in the group! (" + item2.TopParent.UsefulName + ")\n";
				}
			}
			else
			{
				text = text + "Non-mobile group " + item2.id + "\n";
			}
			if (item2.id == -1)
			{
				text += "WARNING: Group was deleted but is still in use by a placeable.\n";
			}
			foreach (AttachmentLink link in item2.links)
			{
				try
				{
					hashSet2.Add(link.Top);
					hashSet2.Add(link.Bottom);
					text += GetLinkInfo(link);
					if (!link.Top.Links.Contains(link))
					{
						text = text + "\t\t\t(not in links for " + link.Top.Piece.UsefulName + "!)\n";
					}
					if (!link.Bottom.Links.Contains(link))
					{
						text = text + "\t\t\t(not in links for " + link.Bottom.Piece.UsefulName + "!)\n";
					}
				}
				catch (Exception)
				{
					text += "\tLink: (Exception!)\n";
				}
			}
			text += "Per-node links:\n";
			foreach (AttachmentNode item3 in hashSet2)
			{
				text = text + "\tNode " + item3.Piece.UsefulName + "\n";
				foreach (AttachmentLink link2 in item3.Links)
				{
					text = text + "\t\t" + link2.Top.Piece.UsefulName + " <-> " + link2.Bottom.Piece.UsefulName + "\n";
				}
			}
			text += "\n";
		}
		Debug.Log(text);
		DoSanityChecks();
	}

	private string GetLinkInfo(AttachmentLink link)
	{
		string text = "\tLink:\t";
		text = ((!(link.Top.Piece != null)) ? (text + "<null>") : (text + "[" + link.Top.Piece.ID + (link.Top.Piece.isSetPiece ? "S" : "") + "] " + link.Top.Piece.name));
		text += " / ";
		text = ((!(link.Bottom.Piece != null)) ? (text + "<null>") : (text + "[" + link.Bottom.Piece.ID + (link.Bottom.Piece.isSetPiece ? "S" : "") + "] " + link.Bottom.Piece.name));
		return text + "\n";
	}

	private void DoSanityChecks()
	{
		HashSet<AttachmentGroup> hashSet = new HashSet<AttachmentGroup>();
		Placeable[] array = UnityEngine.Object.FindObjectsOfType<Placeable>();
		foreach (Placeable placeable in array)
		{
			if (placeable.Group != null)
			{
				hashSet.Add(placeable.Group);
			}
			for (int j = 0; j < placeable.ChildPieces.Count; j++)
			{
				Placeable placeable2 = placeable.ChildPieces[j];
				if (placeable2 != null)
				{
					if (placeable2.ParentPiece != placeable)
					{
						Debug.LogWarning("Sanity Check Warning: " + placeable2.name + " [" + placeable2.ID + "] is child " + j + " of " + placeable.name + " [" + placeable.ID + "] but has a different parent piece: " + ((placeable2.ParentPiece != null) ? (placeable2.ParentPiece.name + " [" + placeable2.ParentPiece.ID + "]") : "NULL"));
					}
				}
				else
				{
					Debug.LogWarning("Sanity Check Warning: " + placeable.name + " [" + placeable.ID + "] has null child at index " + j);
				}
			}
			if (placeable.ParentPiece != null && (placeable.ParentPiece.ChildPieces == null || !placeable.ParentPiece.ChildPieces.Contains(placeable)))
			{
				Debug.LogWarning("Sanity Check Warning: " + placeable.name + " [" + placeable.ID + "] has parent " + placeable.ParentPiece.name + " [" + placeable.ParentPiece.ID + "] but parent has no child entry for it");
			}
		}
		foreach (AttachmentGroup item in hashSet)
		{
			foreach (AttachmentLink link in item.links)
			{
				if (link.Top != null && link.Top.Piece != null)
				{
					SanityCheckGroup(link.Top.Piece, item);
				}
				if (link.Bottom != null && link.Bottom.Piece != null)
				{
					SanityCheckGroup(link.Bottom.Piece, item);
				}
			}
		}
	}

	private void SanityCheckGroup(Placeable placeable, AttachmentGroup group)
	{
		if (placeable.Group != group)
		{
			Debug.LogWarning("Sanity Check Warning: " + placeable.UsefulName + " is in links for group " + group.id + " but has its group set to: " + ((placeable.Group != null) ? (" group id " + placeable.Group.id) : "null"));
		}
	}
}
