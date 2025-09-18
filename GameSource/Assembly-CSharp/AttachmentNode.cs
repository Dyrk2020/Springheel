using System.Collections.Generic;

public class AttachmentNode
{
	public Placeable Piece;

	public bool Marked;

	public List<AttachmentLink> Links;

	public bool Debug;

	public AttachmentNode(Placeable piece)
	{
		Piece = piece;
		Marked = false;
		Links = new List<AttachmentLink>();
		Debug = false;
	}
}
