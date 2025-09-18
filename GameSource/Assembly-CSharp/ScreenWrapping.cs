using System;
using Smooth;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(BoxCollider2D))]
public class ScreenWrapping : MonoBehaviour
{
	private BoxCollider2D boundaryCollider;

	private float minX;

	private float maxX;

	private float minY;

	private float maxY;

	private float wrapWidth;

	private float wrapHeight;

	public float screenWrapInvincibility = 0.1f;

	private void Awake()
	{
		boundaryCollider = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		Bounds bounds = boundaryCollider.bounds;
		minX = bounds.min.x;
		maxX = bounds.max.x;
		minY = bounds.min.y;
		maxY = bounds.max.y;
		wrapWidth = bounds.size.x;
		wrapHeight = bounds.size.y;
	}

	private void Update()
	{
		WrapPlayers();
		WrapProjectiles();
		ManageTeleportersBoundary();
		WrapCoins();
	}

	private void OnEnable()
	{
		Coin.ModifyTargetPosition = (Coin.TargetPositionModifier)Delegate.Combine(Coin.ModifyTargetPosition, new Coin.TargetPositionModifier(CalculateCoinVirtualTargetPosition));
	}

	private void OnDisable()
	{
		Coin.ModifyTargetPosition = (Coin.TargetPositionModifier)Delegate.Remove(Coin.ModifyTargetPosition, new Coin.TargetPositionModifier(CalculateCoinVirtualTargetPosition));
	}

	private Vector3 CalculateCoinVirtualTargetPosition(Vector3 currentPosition, Vector3 targetPosition)
	{
		Vector3 vector = targetPosition;
		float sqrMagnitude = (targetPosition - currentPosition).sqrMagnitude;
		Vector3 vector2 = targetPosition;
		if (targetPosition.x > currentPosition.x)
		{
			vector2.x -= wrapWidth;
		}
		else
		{
			vector2.x += wrapWidth;
		}
		if ((vector2 - currentPosition).sqrMagnitude < sqrMagnitude)
		{
			vector = vector2;
			sqrMagnitude = (vector2 - currentPosition).sqrMagnitude;
		}
		Vector3 vector3 = vector;
		if (vector.y > currentPosition.y)
		{
			vector3.y -= wrapHeight;
		}
		else
		{
			vector3.y += wrapHeight;
		}
		if ((vector3 - currentPosition).sqrMagnitude < sqrMagnitude)
		{
			vector = vector3;
		}
		return vector;
	}

	private void ManageTeleportersBoundary()
	{
		foreach (Teleporter allTeleporter in Teleporter.AllTeleporters)
		{
			if (allTeleporter != null)
			{
				bool isTemporarilyDisabled = IsOutOfBounds(allTeleporter.transform.position);
				allTeleporter.IsTemporarilyDisabled = isTemporarilyDisabled;
			}
		}
	}

	private void WrapPlayers()
	{
		Player[] players = PlayerManager.GetInstance().Players;
		if (players == null || players.Length == 0)
		{
			return;
		}
		Player[] array = players;
		foreach (Player player in array)
		{
			if (player != null && player.PlayerCharacter != null)
			{
				ScreenWrapCharacter(player.PlayerCharacter);
			}
		}
	}

	private void WrapProjectiles()
	{
		Projectile[] array = UnityEngine.Object.FindObjectsOfType<Projectile>();
		foreach (Projectile projectile in array)
		{
			if (projectile != null && projectile.gameObject.activeInHierarchy && ScreenWrapGameObject(projectile.gameObject))
			{
				projectile.NotifyWrapped();
			}
		}
	}

	private void WrapCoins()
	{
		for (int i = 0; i < Coin.AllCoins.Count; i++)
		{
			Coin coin = Coin.AllCoins[i];
			if (coin != null && coin.gameObject.activeInHierarchy)
			{
				ScreenWrapGameObject(coin.gameObject);
			}
		}
	}

	private bool HasNetworkAuthority(GameObject targetGameObject)
	{
		NetworkIdentity component = targetGameObject.GetComponent<NetworkIdentity>();
		if (component != null)
		{
			if (!component.hasAuthority)
			{
				return component.isServer;
			}
			return true;
		}
		return true;
	}

	private bool ScreenWrapGameObject(GameObject gameObjectToWrap)
	{
		if (!HasNetworkAuthority(gameObjectToWrap))
		{
			return false;
		}
		Vector3 position = gameObjectToWrap.transform.position;
		Vector3 wrapOffset = GetWrapOffset(position);
		if (wrapOffset != Vector3.zero)
		{
			TeleportTransform(gameObjectToWrap.transform, position + wrapOffset);
			return true;
		}
		return false;
	}

	private bool ScreenWrapCharacter(Character characterToWrap)
	{
		if (!characterToWrap.hasAuthority)
		{
			return false;
		}
		if (!characterToWrap.Enabled)
		{
			return false;
		}
		Vector3 position = characterToWrap.gameObject.transform.position;
		Vector3 wrapOffset = GetWrapOffset(position);
		if (wrapOffset != Vector3.zero)
		{
			characterToWrap.PositionCharacter(position + wrapOffset);
			characterToWrap.AudioEventExact("SFX_Level_Pictureframe_Character_Warp");
			characterToWrap.StartInvincibleTimer(screenWrapInvincibility);
			return true;
		}
		return false;
	}

	private void TeleportTransform(Transform target, Vector3 newPosition)
	{
		SmoothSync component = target.GetComponent<SmoothSync>();
		if (component != null)
		{
			int approximateNetworkTimeOnOwner = component.approximateNetworkTimeOnOwner;
			component.teleport(approximateNetworkTimeOnOwner, newPosition, target.rotation);
		}
		else
		{
			target.position = newPosition;
		}
	}

	public Vector3 WrapPosition(Vector3 position)
	{
		Vector3 result = position;
		if (position.x < minX)
		{
			result.x += wrapWidth;
		}
		else if (position.x > maxX)
		{
			result.x -= wrapWidth;
		}
		if (position.y < minY)
		{
			result.y += wrapHeight;
		}
		else if (position.y > maxY)
		{
			result.y -= wrapHeight;
		}
		return result;
	}

	public bool IsOutOfBounds(Vector3 position)
	{
		if (!(position.x < minX) && !(position.x > maxX) && !(position.y < minY))
		{
			return position.y > maxY;
		}
		return true;
	}

	public Vector3 GetWrapOffset(Vector3 position)
	{
		Vector3 zero = Vector3.zero;
		if (position.x < minX)
		{
			zero.x = wrapWidth;
		}
		else if (position.x > maxX)
		{
			zero.x = 0f - wrapWidth;
		}
		if (position.y < minY)
		{
			zero.y = wrapHeight;
		}
		else if (position.y > maxY)
		{
			zero.y = 0f - wrapHeight;
		}
		return zero;
	}
}
