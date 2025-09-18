using System.Collections.Generic;
using UnityEngine;

public class CollisionTag : MonoBehaviour
{
	[BitMask(typeof(TagComparer.Tag))]
	public TagComparer.Tag bitMask;

	public static readonly Dictionary<Collider2D, CollisionTag> AllTags = new Dictionary<Collider2D, CollisionTag>();

	private Collider2D[] myColliders;

	private void Awake()
	{
		myColliders = GetComponents<Collider2D>();
	}

	private void OnEnable()
	{
		Collider2D[] array = myColliders;
		foreach (Collider2D key in array)
		{
			AllTags[key] = this;
		}
	}

	private void OnDisable()
	{
		Collider2D[] array = myColliders;
		foreach (Collider2D key in array)
		{
			AllTags.Remove(key);
		}
	}

	public TagComparer.Tag CalculateBitMask()
	{
		HashSet<TagComparer.Tag> splitTag = TagComparer.GetSplitTag(base.gameObject.tag);
		TagComparer.Tag tag = TagComparer.Tag.None;
		foreach (TagComparer.Tag item in splitTag)
		{
			tag |= item;
		}
		return tag;
	}

	public bool ContainsAnyTag(TagComparer.Tag tag)
	{
		return (bitMask & tag) != 0;
	}

	public bool ContainsAnyTag(int tag)
	{
		return ((uint)bitMask & (uint)tag) != 0;
	}

	public bool ContainsAllTags(TagComparer.Tag mask)
	{
		return (bitMask & mask) == mask;
	}

	public bool ContainsAllTags(int mask)
	{
		return ((uint)bitMask & (uint)mask) == (uint)mask;
	}

	public bool ExactTagMatch(TagComparer.Tag tag)
	{
		return bitMask == tag;
	}
}
