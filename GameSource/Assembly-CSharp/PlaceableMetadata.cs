using System.Collections.Generic;
using UnityEngine;

public class PlaceableMetadata : MonoBehaviour
{
	public int blockSerializeIndex = -1;

	public bool isLevelGeometry;

	public Placeable placeableRef;

	public List<MultipiecePart> attachmentPoints = new List<MultipiecePart>();

	public List<MultipiecePart> subElements = new List<MultipiecePart>();
}
