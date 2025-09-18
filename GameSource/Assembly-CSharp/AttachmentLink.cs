public struct AttachmentLink
{
	public AttachmentNode Top;

	public AttachmentNode Bottom;

	public AttachmentLink(AttachmentNode top, AttachmentNode bottom)
	{
		Top = top;
		Bottom = bottom;
		Top.Links.Add(this);
		Bottom.Links.Add(this);
	}

	public AttachmentLink(Placeable top, Placeable bottom)
	{
		Top = new AttachmentNode(top);
		Bottom = new AttachmentNode(bottom);
		Top.Links.Add(this);
		Bottom.Links.Add(this);
	}
}
