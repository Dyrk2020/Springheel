using UnityEngine;

public class Spraypaint : Bomb
{
	public override void EnablePlacement(bool showGuides = true)
	{
		base.EnablePlacement(showGuides);
		Killzone.TintColor = CustomColor;
	}

	protected override void onExplode()
	{
		bombExploded = true;
		GameObject[] array = Killzone.InBlastZone.ToArray();
		foreach (GameObject gameObject in array)
		{
			if (!(gameObject == null))
			{
				Placeable componentInParent = gameObject.GetComponentInParent<Placeable>();
				if ((bool)componentInParent && componentInParent.canSetCustomColor)
				{
					componentInParent.SetColor(CustomColor);
					componentInParent.RemoveBombTint();
					componentInParent.Protected = false;
				}
			}
		}
	}

	public override void SetColor(Color newColor)
	{
		SpriteRenderer[] placementGuides = PlacementGuides;
		for (int i = 0; i < placementGuides.Length; i++)
		{
			placementGuides[i].color = newColor;
		}
		base.SetColor(newColor);
	}

	public override void Place(int playerNumber)
	{
		placedByPlayerNumber = playerNumber;
		placed = false;
		if (bombArmed)
		{
			BombAnimator.SetBool("Placed", value: true);
			AkSoundEngine.PostEvent(FuseSoundString, base.gameObject);
		}
	}
}
