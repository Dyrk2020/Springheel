using System;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentGroup
{
	public List<AttachmentLink> links;

	protected AttachmentNode topNode;

	private bool drawingDebug;

	public int id;

	public static int sequenceNumber;

	public Placeable TopParent
	{
		get
		{
			if (topNode != null)
			{
				return topNode.Piece;
			}
			return null;
		}
	}

	public int PieceCount { get; protected set; }

	public bool ContainsSetPieces
	{
		get
		{
			for (int i = 0; i != links.Count; i++)
			{
				AttachmentLink attachmentLink = links[i];
				if (attachmentLink.Top.Piece != null && attachmentLink.Top.Piece.isSetPiece)
				{
					return true;
				}
				if (attachmentLink.Bottom.Piece != null && attachmentLink.Bottom.Piece.isSetPiece)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool ContainsNonSetPieces
	{
		get
		{
			for (int i = 0; i != links.Count; i++)
			{
				AttachmentLink attachmentLink = links[i];
				if (attachmentLink.Top.Piece != null && !attachmentLink.Top.Piece.isSetPiece)
				{
					return true;
				}
				if (attachmentLink.Bottom.Piece != null && !attachmentLink.Bottom.Piece.isSetPiece)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool ContainsMixedPieces
	{
		get
		{
			if (links.Count == 0)
			{
				return false;
			}
			bool isSetPiece = links[0].Top.Piece.isSetPiece;
			for (int i = 0; i != links.Count; i++)
			{
				AttachmentLink attachmentLink = links[i];
				if (attachmentLink.Top.Piece != null && attachmentLink.Top.Piece.isSetPiece != isSetPiece)
				{
					return true;
				}
				if (attachmentLink.Bottom.Piece != null && attachmentLink.Bottom.Piece.isSetPiece != isSetPiece)
				{
					return true;
				}
			}
			return false;
		}
	}

	private HashSet<AttachmentNode> AllNodes
	{
		get
		{
			HashSet<AttachmentNode> hashSet = new HashSet<AttachmentNode>();
			foreach (AttachmentLink link in links)
			{
				if (link.Top != null)
				{
					hashSet.Add(link.Top);
				}
				if (link.Bottom != null)
				{
					hashSet.Add(link.Bottom);
				}
			}
			return hashSet;
		}
	}

	public AttachmentGroup()
	{
		links = new List<AttachmentLink>();
		id = sequenceNumber++;
	}

	public AttachmentGroup(Placeable top, Placeable bottom)
	{
		AttachmentNode top2 = new AttachmentNode(top);
		AttachmentNode bottom2 = new AttachmentNode(bottom);
		AttachmentLink link = new AttachmentLink(top2, bottom2);
		links = new List<AttachmentLink>();
		AddLink(link);
		top.Group = this;
		bottom.Group = this;
		PieceCount = 2;
		id = sequenceNumber++;
		if (top.IsMobileBlock)
		{
			topNode = top2;
		}
		else if (bottom.IsMobileBlock)
		{
			topNode = bottom2;
		}
	}

	public void ForceAddLink(Placeable a, Placeable b)
	{
		foreach (AttachmentLink link in links)
		{
			if ((link.Top.Piece == a && link.Bottom.Piece == b) || (link.Top.Piece == b && link.Bottom.Piece == a))
			{
				return;
			}
		}
		links.Add(new AttachmentLink(a, b));
	}

	public bool AddLinkNoAttach(Placeable inGroup, Placeable newPiece, bool newIsTop = false)
	{
		AttachmentNode attachmentNode = IsConnected(inGroup);
		if (attachmentNode != null)
		{
			AttachmentNode attachmentNode2 = IsConnected(newPiece);
			if (attachmentNode2 == null)
			{
				attachmentNode2 = new AttachmentNode(newPiece);
			}
			AttachmentLink link = new AttachmentLink(newIsTop ? attachmentNode2 : attachmentNode, newIsTop ? attachmentNode : attachmentNode2);
			if (AddLink(link))
			{
				newPiece.Group = this;
				PieceCount++;
				return true;
			}
			return false;
		}
		return false;
	}

	public bool AddLink(Placeable inGroup, Placeable newPiece, bool newIsTop = false)
	{
		AttachmentNode attachmentNode = inGroup.Group.FindNodeForPlaceable(inGroup);
		if (attachmentNode != null)
		{
			AttachmentNode attachmentNode2 = ((newPiece.Group != null) ? newPiece.Group.FindNodeForPlaceable(newPiece) : null);
			if (attachmentNode2 == null)
			{
				attachmentNode2 = new AttachmentNode(newPiece);
			}
			AttachmentLink link = new AttachmentLink(newIsTop ? attachmentNode2 : attachmentNode, newIsTop ? attachmentNode : attachmentNode2);
			if (AddLink(link))
			{
				newPiece.Group = this;
				if (TopParent != null && !newPiece.AttachesToParent)
				{
					TopParent.AttachPiece(newPiece);
				}
				PieceCount++;
				return true;
			}
			Debug.LogWarning("Link not added");
			return false;
		}
		Debug.LogWarning("Link not added; " + inGroup.UsefulName + " has no existing node.");
		return false;
	}

	public bool AddLink(AttachmentLink link)
	{
		foreach (AttachmentLink link2 in links)
		{
			if (link2.Top.Piece == link.Top.Piece)
			{
				if (link2.Bottom.Piece == link.Bottom.Piece)
				{
					Debug.LogError("Link between " + link2.Bottom.Piece.UsefulName + " and " + link2.Top.Piece.UsefulName + " not added to group " + id + ": Already exists");
					return false;
				}
			}
			else if (link2.Top.Piece == link.Bottom.Piece && link2.Bottom.Piece == link.Top.Piece)
			{
				Debug.LogError("Link between " + link2.Bottom.Piece.UsefulName + " and " + link2.Top.Piece.UsefulName + " not added to group " + id + ": Already exists");
				return false;
			}
		}
		if (TopParent == null)
		{
			links.Add(link);
			if (link.Top.Piece.IsMobileBlock)
			{
				SetTopParent(link.Top);
			}
			else if (link.Bottom.Piece.IsMobileBlock)
			{
				SetTopParent(link.Bottom);
			}
			return true;
		}
		links.Add(link);
		return true;
	}

	protected void SetTopParent(AttachmentNode n)
	{
		AttachmentLink[] array;
		if (n != null)
		{
			n.Piece.Group = this;
			topNode = n;
			array = links.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				AttachmentLink attachmentLink = array[i];
				if (!attachmentLink.Top.Piece.AttachesToParent)
				{
					n.Piece.AttachPiece(attachmentLink.Top.Piece);
				}
				if (!attachmentLink.Bottom.Piece.AttachesToParent)
				{
					n.Piece.AttachPiece(attachmentLink.Bottom.Piece);
				}
			}
			return;
		}
		array = links.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			AttachmentLink attachmentLink2 = array[i];
			if (!attachmentLink2.Top.Piece.AttachesToParent)
			{
				TopParent.DetachPiece(attachmentLink2.Top.Piece, removeFromGroup: true);
			}
			if (!attachmentLink2.Bottom.Piece.AttachesToParent)
			{
				TopParent.DetachPiece(attachmentLink2.Bottom.Piece, removeFromGroup: true);
			}
		}
	}

	public AttachmentNode IsConnected(Placeable p)
	{
		if (TopParent != null)
		{
			return isConnectedTo(topNode, p);
		}
		if (links.Count > 0)
		{
			return isConnectedTo(links[0].Top, p);
		}
		return null;
	}

	protected AttachmentNode isConnectedTo(AttachmentNode root, Placeable p)
	{
		ResetMarks();
		Queue<AttachmentNode> queue = new Queue<AttachmentNode>();
		queue.Enqueue(root);
		root.Marked = true;
		while (queue.Count > 0)
		{
			AttachmentNode attachmentNode = queue.Dequeue();
			if (attachmentNode.Piece == p)
			{
				return attachmentNode;
			}
			foreach (AttachmentLink link in attachmentNode.Links)
			{
				AttachmentNode attachmentNode2 = ((!(link.Top.Piece == attachmentNode.Piece)) ? link.Top : link.Bottom);
				if (!attachmentNode2.Marked)
				{
					attachmentNode2.Marked = true;
					queue.Enqueue(attachmentNode2);
				}
			}
		}
		return null;
	}

	public void DestroyGroup()
	{
		if (TopParent != null && TopParent.Group == this)
		{
			TopParent.Group = null;
		}
		foreach (AttachmentLink link in links)
		{
			if (link.Top.Piece != null && link.Bottom.Piece != null)
			{
				if (link.Top.Piece.ChildPieces.Contains(link.Bottom.Piece) && !link.Bottom.Piece.GetComponent<MultipiecePart>())
				{
					link.Top.Piece.ChildPieces.Remove(link.Bottom.Piece);
				}
				if (link.Bottom.Piece.ChildPieces.Contains(link.Top.Piece) && !link.Top.Piece.GetComponent<MultipiecePart>())
				{
					link.Bottom.Piece.ChildPieces.Remove(link.Top.Piece);
				}
			}
			if (link.Top.Piece != null)
			{
				link.Top.Piece.Group = null;
			}
			if (link.Bottom.Piece != null)
			{
				link.Bottom.Piece.Group = null;
			}
			if (!link.Top.Piece.GetComponent<MultipiecePart>())
			{
				link.Top.Piece.ParentPiece = null;
				link.Top.Piece.transform.parent = null;
			}
			if (!link.Bottom.Piece.GetComponent<MultipiecePart>())
			{
				link.Bottom.Piece.ParentPiece = null;
				link.Bottom.Piece.transform.parent = null;
			}
		}
		links.Clear();
		id = -1;
	}

	protected void ResetMarks()
	{
		for (int i = 0; i != links.Count; i++)
		{
			AttachmentLink attachmentLink = links[i];
			attachmentLink.Top.Marked = false;
			attachmentLink.Bottom.Marked = false;
		}
	}

	protected void ResetDebug()
	{
		for (int i = 0; i != links.Count; i++)
		{
			AttachmentLink attachmentLink = links[i];
			attachmentLink.Top.Debug = false;
			attachmentLink.Bottom.Debug = false;
		}
	}

	public PhysicsModifier[] GetGroupPhysicsModifier()
	{
		if (TopParent != null)
		{
			return TopParent.GetPhysicsModifier();
		}
		return null;
	}

	public static AttachmentGroup MergeGroups(AttachmentGroup g1, AttachmentGroup g2)
	{
		if (g1 == g2)
		{
			return null;
		}
		AttachmentGroup attachmentGroup;
		AttachmentGroup attachmentGroup2;
		if (g1.topNode != null && g2.topNode == null)
		{
			attachmentGroup = g2;
			attachmentGroup2 = g1;
		}
		else if (g1.topNode == null && g2.topNode != null)
		{
			attachmentGroup = g1;
			attachmentGroup2 = g2;
		}
		else
		{
			if (!(g1.TopParent == g2.TopParent))
			{
				return null;
			}
			attachmentGroup = g2;
			attachmentGroup2 = g1;
		}
		for (int i = 0; i < attachmentGroup.links.Count; i++)
		{
			AttachmentLink item = attachmentGroup.links[i];
			attachmentGroup2.links.Add(item);
			if (attachmentGroup2.TopParent != null)
			{
				if (!item.Top.Piece.AttachesToParent)
				{
					attachmentGroup2.TopParent.AttachPiece(item.Top.Piece);
				}
				if (!item.Bottom.Piece.AttachesToParent)
				{
					attachmentGroup2.TopParent.AttachPiece(item.Bottom.Piece);
				}
			}
			item.Top.Piece.Group = attachmentGroup2;
			item.Bottom.Piece.Group = attachmentGroup2;
		}
		attachmentGroup.links.Clear();
		return attachmentGroup2;
	}

	public void DrawLinks()
	{
		if (drawingDebug)
		{
			return;
		}
		drawingDebug = true;
		ResetDebug();
		AttachmentNode attachmentNode = topNode;
		if (attachmentNode == null)
		{
			if (links.Count <= 0)
			{
				return;
			}
			attachmentNode = links[0].Top;
			if (attachmentNode == null)
			{
				attachmentNode = links[0].Bottom;
			}
		}
		if (attachmentNode == null)
		{
			return;
		}
		Queue<AttachmentNode> queue = new Queue<AttachmentNode>();
		queue.Enqueue(attachmentNode);
		attachmentNode.Debug = true;
		Color red = Color.red;
		while (queue.Count > 0)
		{
			AttachmentNode attachmentNode2 = queue.Dequeue();
			foreach (AttachmentLink link in attachmentNode2.Links)
			{
				AttachmentNode attachmentNode3 = ((!(link.Top.Piece == attachmentNode2.Piece)) ? link.Top : link.Bottom);
				Debug.DrawLine(color: (!attachmentNode2.Piece.HasReverseAttachments && !attachmentNode3.Piece.HasReverseAttachments) ? ((attachmentNode2 != attachmentNode && attachmentNode3 != attachmentNode) ? Color.red : Color.green) : Color.yellow, start: attachmentNode3.Piece.transform.position, end: attachmentNode2.Piece.transform.position, duration: 1f / 6f);
				if (!attachmentNode3.Debug)
				{
					attachmentNode3.Debug = true;
					queue.Enqueue(attachmentNode3);
				}
			}
		}
		drawingDebug = false;
	}

	public bool SetPieceConnected(Placeable p)
	{
		if (!p.isSetPiece || TopParent == null || !TopParent.IsMobileBlock || !TopParent.isSetPiece)
		{
			return false;
		}
		ResetMarks();
		Queue<AttachmentNode> queue = new Queue<AttachmentNode>();
		queue.Enqueue(topNode);
		topNode.Marked = true;
		while (queue.Count > 0)
		{
			AttachmentNode attachmentNode = queue.Dequeue();
			if (attachmentNode.Piece == p)
			{
				return true;
			}
			foreach (AttachmentLink link in attachmentNode.Links)
			{
				AttachmentNode attachmentNode2 = ((link.Top == attachmentNode) ? link.Bottom : link.Top);
				if (!attachmentNode2.Marked && attachmentNode2.Piece.isSetPiece)
				{
					queue.Enqueue(attachmentNode2);
					attachmentNode2.Marked = true;
				}
			}
		}
		return false;
	}

	public IEnumerable<AttachmentNode> FindNodesForPlaceable(Placeable p)
	{
		if (p.Group != this)
		{
			if (p.Group != null)
			{
				Debug.LogError("FindNodeForPlaceable: Wrong group! " + p.UsefulName + " is in group " + p.Group.id + " -- this is group " + id);
			}
			else
			{
				Debug.LogError("FindNodeForPlaceable: Wrong group! " + p.UsefulName + " is not in a group");
			}
			yield break;
		}
		HashSet<AttachmentNode> seenNodes = new HashSet<AttachmentNode>();
		foreach (AttachmentLink link in links)
		{
			if (link.Top.Piece == p && !seenNodes.Contains(link.Top))
			{
				seenNodes.Add(link.Top);
				yield return link.Top;
			}
			if (link.Bottom.Piece == p && !seenNodes.Contains(link.Bottom))
			{
				seenNodes.Add(link.Bottom);
				yield return link.Bottom;
			}
		}
	}

	public AttachmentNode FindNodeForPlaceable(Placeable p)
	{
		if (p.Group != this)
		{
			Debug.LogError("FindNodeForPlaceable: Wrong group!");
			return null;
		}
		foreach (AttachmentLink link in links)
		{
			if (link.Top.Piece == p)
			{
				return link.Top;
			}
			if (link.Bottom.Piece == p)
			{
				return link.Bottom;
			}
		}
		return null;
	}

	public Placeable FindFirstConnectedSetPiece(Placeable p)
	{
		if (p.Group != this)
		{
			Debug.LogError("FindFirstConnectedSetPiece: Wrong group!");
			return null;
		}
		if (p.isSetPiece)
		{
			Debug.LogError("FindFirstConnectedSetPiece: p.isSetPiece == true");
			return null;
		}
		if (p.isSetPiece || TopParent == null || !TopParent.IsMobileBlock || !TopParent.isSetPiece)
		{
			return null;
		}
		Queue<AttachmentNode> queue = new Queue<AttachmentNode>();
		HashSet<AttachmentLink> hashSet = new HashSet<AttachmentLink>();
		foreach (AttachmentNode item2 in FindNodesForPlaceable(p))
		{
			queue.Enqueue(item2);
		}
		if (queue.Count == 0)
		{
			Debug.LogError("FindFirstConnectedSetPiece: Can't find start node");
			return null;
		}
		while (queue.Count > 0)
		{
			AttachmentNode attachmentNode = queue.Dequeue();
			if (attachmentNode.Piece.isSetPiece)
			{
				if (SetPieceConnected(attachmentNode.Piece))
				{
					return attachmentNode.Piece;
				}
				continue;
			}
			foreach (AttachmentLink link in attachmentNode.Links)
			{
				if (!hashSet.Contains(link))
				{
					AttachmentNode item = ((link.Top == attachmentNode) ? link.Bottom : link.Top);
					queue.Enqueue(item);
					hashSet.Add(link);
				}
			}
		}
		return null;
	}

	public void RemovePlaceable(Placeable p)
	{
		SimplifyNodes();
		HashSet<AttachmentNode> hashSet = new HashSet<AttachmentNode>();
		foreach (AttachmentNode item in FindNodesForPlaceable(p))
		{
			hashSet.Add(item);
		}
		foreach (AttachmentNode allNode in AllNodes)
		{
			if (!allNode.Piece.AttachedBy.Contains(p))
			{
				continue;
			}
			if (allNode.Piece.AttachedBy.Count == 1)
			{
				if (p.AttachesToParent && p.ParentPiece != allNode.Piece.ParentPiece)
				{
					if (TopParent == allNode.Piece)
					{
						AttachmentNode attachmentNode = FindNodeForPlaceable(p.ParentPiece);
						if (attachmentNode != null)
						{
							hashSet.Add(attachmentNode);
						}
					}
					else
					{
						hashSet.Add(allNode);
					}
				}
				else if (allNode.Piece != TopParent)
				{
					hashSet.Add(allNode);
				}
			}
			allNode.Piece.AttachedBy.Remove(p);
		}
		HashSet<AttachmentLink> pLinks = new HashSet<AttachmentLink>();
		foreach (AttachmentNode item2 in hashSet)
		{
			item2.Piece.Group = null;
			foreach (AttachmentLink link in item2.Links)
			{
				Placeable piece = link.Top.Piece;
				Placeable piece2 = link.Bottom.Piece;
				if ((piece.AttachesToParent && piece != p && piece.ParentPiece == piece2) || (piece2.AttachesToParent && piece2 != p && piece2.ParentPiece == piece))
				{
					continue;
				}
				pLinks.Add(link);
				AttachmentNode attachmentNode2 = null;
				if (item2 == link.Top)
				{
					attachmentNode2 = link.Bottom;
				}
				else if (item2 == link.Bottom)
				{
					attachmentNode2 = link.Top;
				}
				if (attachmentNode2 != null)
				{
					attachmentNode2.Links.Remove(link);
					if (attachmentNode2.Links.Count == 0)
					{
						attachmentNode2.Piece.Group = null;
					}
				}
			}
			item2.Links.Clear();
		}
		HashSet<AttachmentNode> allNodes = AllNodes;
		links.RemoveAll((AttachmentLink link) => pLinks.Contains(link));
		SimplifyNodes();
		CleanBrokenLinks(allNodes);
		if (links.Count == 0)
		{
			DestroyGroup();
		}
	}

	private void CleanBrokenLinks(HashSet<AttachmentNode> nodes)
	{
		Dictionary<Placeable, List<AttachmentNode>> directAttachments = new Dictionary<Placeable, List<AttachmentNode>>();
		Action<Placeable, List<AttachmentNode>> action = delegate(Placeable nodePiece, List<AttachmentNode> group)
		{
			if (directAttachments.ContainsKey(nodePiece))
			{
				foreach (AttachmentNode item in directAttachments[nodePiece])
				{
					group.Add(item);
				}
			}
		};
		HashSet<AttachmentNode> hashSet = new HashSet<AttachmentNode>();
		foreach (AttachmentNode node in nodes)
		{
			if (!(node.Piece != null) || node.Piece.MarkedForDestruction || node.Piece.PickedUp || IsConnected(node.Piece) != null)
			{
				continue;
			}
			if (node.Piece.AttachesToParent && node.Piece.ParentPiece != null)
			{
				if (!directAttachments.ContainsKey(node.Piece.ParentPiece))
				{
					directAttachments[node.Piece.ParentPiece] = new List<AttachmentNode> { node };
				}
				else
				{
					directAttachments[node.Piece.ParentPiece].Add(node);
				}
			}
			else
			{
				hashSet.Add(node);
			}
		}
		List<List<AttachmentNode>> list = new List<List<AttachmentNode>>();
		foreach (AttachmentNode item2 in hashSet)
		{
			if (list.Count == 0)
			{
				List<AttachmentNode> list2 = new List<AttachmentNode>();
				list.Add(list2);
				list2.Add(item2);
				action(item2.Piece, list2);
				continue;
			}
			bool flag = false;
			for (int num = 0; num < list.Count; num++)
			{
				List<AttachmentNode> list3 = list[num];
				if (isConnectedTo(list3[0], item2.Piece) != null)
				{
					flag = true;
					list3.Add(item2);
					action(item2.Piece, list3);
					break;
				}
			}
			if (!flag)
			{
				List<AttachmentNode> list4 = new List<AttachmentNode>();
				list.Add(list4);
				list4.Add(item2);
				action(item2.Piece, list4);
			}
		}
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			List<AttachmentNode> list5 = list[num2];
			if (list5.Count <= 1)
			{
				if (list5.Count == 1)
				{
					Placeable piece = list5[0].Piece;
					if (piece.ParentPiece != null)
					{
						piece.ParentPiece.DetachPiece(piece, removeFromGroup: false);
						piece.Group = null;
					}
				}
				continue;
			}
			HashSet<Placeable> hashSet2 = new HashSet<Placeable>();
			foreach (AttachmentNode item3 in list5)
			{
				if (item3.Piece != null)
				{
					hashSet2.Add(item3.Piece);
				}
			}
			bool flag2 = false;
			foreach (Placeable item4 in hashSet2)
			{
				if (item4.IsMobileBlock)
				{
					flag2 = true;
					break;
				}
			}
			foreach (AttachmentNode item5 in list5)
			{
				Placeable piece2 = item5.Piece;
				if (piece2.ParentPiece != null && !hashSet2.Contains(piece2.ParentPiece))
				{
					piece2.ParentPiece.DetachPiece(piece2, removeFromGroup: false);
				}
			}
			HashSet<AttachmentLink> involvedLinks = new HashSet<AttachmentLink>();
			foreach (AttachmentLink link in links)
			{
				if (hashSet2.Contains(link.Bottom.Piece) || hashSet2.Contains(link.Top.Piece))
				{
					involvedLinks.Add(link);
				}
			}
			foreach (AttachmentLink item6 in involvedLinks)
			{
				item6.Bottom.Links.Remove(item6);
				item6.Top.Links.Remove(item6);
			}
			links.RemoveAll((AttachmentLink link) => involvedLinks.Contains(link));
			Dictionary<Placeable, AttachmentNode> dictionary = new Dictionary<Placeable, AttachmentNode>();
			foreach (AttachmentLink item7 in involvedLinks)
			{
				if (item7.Top.Piece != null && !dictionary.ContainsKey(item7.Top.Piece))
				{
					dictionary.Add(item7.Top.Piece, new AttachmentNode(item7.Top.Piece));
				}
				if (item7.Bottom.Piece != null && !dictionary.ContainsKey(item7.Bottom.Piece))
				{
					dictionary.Add(item7.Bottom.Piece, new AttachmentNode(item7.Bottom.Piece));
				}
			}
			AttachmentGroup attachmentGroup = new AttachmentGroup();
			foreach (AttachmentLink item8 in involvedLinks)
			{
				Placeable piece3 = item8.Top.Piece;
				Placeable piece4 = item8.Bottom.Piece;
				if (dictionary.TryGetValue(piece3, out var value) && dictionary.TryGetValue(piece4, out var value2))
				{
					if (!piece3.AttachesToParent && !piece4.AttachesToParent && flag2)
					{
						piece4.AttachPiece(piece3);
					}
					attachmentGroup.links.Add(new AttachmentLink(value, value2));
					piece3.Group = attachmentGroup;
					piece4.Group = attachmentGroup;
				}
			}
			attachmentGroup.PieceCount = dictionary.Count;
		}
	}

	private void SimplifyNodes()
	{
		Dictionary<Placeable, AttachmentNode> dictionary = new Dictionary<Placeable, AttachmentNode>();
		for (int i = 0; i < links.Count; i++)
		{
			AttachmentNode top = links[i].Top;
			if (top != null && top.Piece != null && !dictionary.ContainsKey(top.Piece))
			{
				dictionary.Add(top.Piece, top);
			}
			AttachmentNode bottom = links[i].Bottom;
			if (bottom != null && bottom.Piece != null && !dictionary.ContainsKey(bottom.Piece))
			{
				dictionary.Add(bottom.Piece, bottom);
			}
		}
		foreach (KeyValuePair<Placeable, AttachmentNode> item in dictionary)
		{
			item.Value.Links.Clear();
		}
		for (int j = 0; j < links.Count; j++)
		{
			AttachmentNode top2 = links[j].Top;
			AttachmentNode bottom2 = links[j].Bottom;
			dictionary.TryGetValue(top2.Piece, out var value);
			dictionary.TryGetValue(bottom2.Piece, out var value2);
			if (value != null && value2 != null)
			{
				if (value != top2 || value2 != bottom2)
				{
					links[j] = new AttachmentLink(value, value2);
				}
				value.Links.Add(links[j]);
				value2.Links.Add(links[j]);
			}
		}
	}
}
