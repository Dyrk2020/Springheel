using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UsableProp : MonoBehaviour
{
	public enum LobbyType
	{
		ALL,
		LOCAL,
		ONLINE
	}

	public delegate void markInUse(bool inUse);

	protected bool inUse;

	public SpriteRenderer spriteRenderer;

	protected bool highLight;

	public List<Character> hoverCharacters = new List<Character>();

	public LobbyType ShowInLobby;

	protected bool hidden;

	public InputEvent.InputKey usedInputKey = InputEvent.InputKey.NoKey;

	public Character characterUsing;

	public event markInUse EventMarkInUse;

	private void CmdMarkInUse(bool inUse)
	{
		this.EventMarkInUse(inUse);
	}

	protected virtual void Start()
	{
		if (NetworkClient.active)
		{
			EventMarkInUse += authSetUsed;
		}
	}

	public virtual void Update()
	{
		Tint();
		if (!(LobbyManager.instance != null) || ShowInLobby == LobbyType.ALL)
		{
			return;
		}
		if (ShowInLobby == LobbyType.LOCAL)
		{
			if (!LobbyManager.instance.IsInOnlineGame)
			{
				show();
			}
			else
			{
				hide();
			}
		}
		else if (!LobbyManager.instance.IsInOnlineGame)
		{
			hide();
		}
		else
		{
			show();
		}
	}

	public virtual bool Use(LobbyPlayer lobbyPlayer, InputEvent.InputKey usedInputKey)
	{
		if (inUse || hidden || lobbyPlayer.CharacterInstance.InMenu)
		{
			return false;
		}
		this.usedInputKey = usedInputKey;
		inUse = true;
		characterUsing = lobbyPlayer.CharacterInstance;
		characterUsing.InMenu = true;
		characterUsing.Freeze(hide: false, freezeAnimator: false, disableColliders: false);
		return true;
	}

	public virtual void Release(bool unFreeze = true)
	{
		inUse = false;
		if (characterUsing != null)
		{
			if (unFreeze)
			{
				characterUsing.InMenu = false;
				characterUsing.Unfreeze();
			}
			characterUsing = null;
		}
	}

	private void authSetUsed(bool inUse)
	{
		this.inUse = inUse;
	}

	private void OnTriggerStay2D(Collider2D c)
	{
		Character component = c.gameObject.GetComponent<Character>();
		if (!(component != null))
		{
			return;
		}
		if ((Mathf.Abs(component.GetComponent<Rigidbody2D>().velocity.x) < 0.5f && component.right < 0.1f && component.left < 0.1f) || component.InMenu)
		{
			component.SetUseableProp(this);
			if (!hoverCharacters.Contains(component))
			{
				hoverCharacters.Add(component);
			}
			return;
		}
		if (component.GetUseableProp() == this)
		{
			component.SetUseableProp(null);
		}
		if (hoverCharacters.Contains(component))
		{
			hoverCharacters.Remove(component);
		}
	}

	private void OnTriggerExit2D(Collider2D c)
	{
		Character component = c.gameObject.GetComponent<Character>();
		if (component != null)
		{
			if (component.GetUseableProp() == this)
			{
				component.SetUseableProp(null);
			}
			if (hoverCharacters.Contains(component))
			{
				hoverCharacters.Remove(component);
			}
		}
	}

	public void Tint()
	{
		Color color = GameSettings.GetInstance().neutralColor;
		if (hoverCharacters.Count >= 1 && !inUse)
		{
			color = GameSettings.GetInstance().highlightColor2;
		}
		spriteRenderer.color = color;
	}

	private void show()
	{
		if (hidden)
		{
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
			hidden = false;
		}
	}

	private void hide()
	{
		if (!hidden)
		{
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			Canvas[] componentsInChildren2 = GetComponentsInChildren<Canvas>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = false;
			}
			hidden = true;
		}
	}
}
