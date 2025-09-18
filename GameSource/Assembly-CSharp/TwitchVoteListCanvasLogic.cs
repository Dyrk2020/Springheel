using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class TwitchVoteListCanvasLogic : MonoBehaviour
{
	public Object voteDisplayPanelPrefab;

	public GameObject noVotesPanel;

	public Animator noVotesPanelAnimator;

	public GameObject voteEntryList;

	public GameObject widgetContainer;

	public GameObject mustBeInPartyModeContainer;

	private List<TwitchVoteDisplayLogic> voteDisplayPanels = new List<TwitchVoteDisplayLogic>();

	private List<TwitchChatClientState.VoteState> lastVoteStates;

	private bool animatingDontUpdateContent;

	public Text channelName;

	public int numberOfVotes;

	public Text numberOfVotesText;

	private Canvas canvas;

	private void Awake()
	{
		canvas = GetComponent<Canvas>();
		for (int i = 0; i < TwitchChatController.NumVoteDisplayWidgets; i++)
		{
			TwitchVoteDisplayLogic twitchVoteDisplayLogic = voteEntryList.AddPrefabAsChild<TwitchVoteDisplayLogic>(voteDisplayPanelPrefab);
			voteDisplayPanels.Add(twitchVoteDisplayLogic);
			twitchVoteDisplayLogic.ShowContents(show: false);
		}
	}

	private void Start()
	{
		UpdateNumVotes();
	}

	private void Update()
	{
		if (canvas.worldCamera == null)
		{
			RefreshCamera();
		}
	}

	public void SetVisible(bool show, bool isInPartyMode, bool IsInGame)
	{
		if (base.gameObject.activeSelf != show)
		{
			base.gameObject.SetActive(show);
			if (show)
			{
				RefreshCamera();
			}
		}
		mustBeInPartyModeContainer.SetActive(show && !isInPartyMode && !IsInGame);
		widgetContainer.SetActive(show && isInPartyMode);
	}

	private void RefreshCamera()
	{
		canvas.worldCamera = LobbyManager.instance.GetCurrentUICamera();
	}

	public void UpdateVotesFromClientState(TwitchChatClientState state)
	{
		if (animatingDontUpdateContent)
		{
			return;
		}
		if (lastVoteStates == null)
		{
			lastVoteStates = new List<TwitchChatClientState.VoteState>();
			for (int i = 0; i < voteDisplayPanels.Count; i++)
			{
				lastVoteStates.Add(new TwitchChatClientState.VoteState(voteDisplayPanels[i].lastPickableIndex, voteDisplayPanels[i].lastVoteCount, newVotes: false));
			}
		}
		bool flag = false;
		for (int j = 0; j < voteDisplayPanels.Count; j++)
		{
			bool flag2 = state.SyncListVoteStates[j].pickableIndex != -1;
			voteDisplayPanels[j].ShowContents(flag2);
			if (flag2)
			{
				flag = true;
			}
		}
		if (!noVotesPanel.activeInHierarchy && !flag)
		{
			noVotesPanel.SetActive(!flag);
			if (noVotesPanelAnimator.isInitialized)
			{
				noVotesPanelAnimator.SetTrigger("FadeIn");
			}
		}
		else
		{
			noVotesPanel.SetActive(!flag);
		}
		if (channelName.text != state.channelName)
		{
			channelName.text = state.channelName;
		}
		if (numberOfVotes != state.NumberOfVotes)
		{
			numberOfVotes = state.NumberOfVotes;
			UpdateNumVotes();
		}
		int count = state.SyncListVoteStates.Count;
		for (int k = 0; k < count; k++)
		{
			bool flag3 = false;
			int pickableIndex = state.SyncListVoteStates[k].pickableIndex;
			if (pickableIndex != lastVoteStates[k].pickableIndex)
			{
				if (pickableIndex != -1)
				{
					voteDisplayPanels[k].SetItemName(TwitchChatController.itemShortNames[pickableIndex], pickableIndex);
				}
				else
				{
					voteDisplayPanels[k].SetItemName("None", -1);
				}
				flag3 = true;
			}
			int votes = state.SyncListVoteStates[k].votes;
			if (votes != lastVoteStates[k].votes)
			{
				voteDisplayPanels[k].SetVoteCount(votes);
				flag3 = true;
			}
			if (lastVoteStates[k].newVotes != state.SyncListVoteStates[k].newVotes)
			{
				voteDisplayPanels[k].TriggerNewVoteAnimation();
			}
			if (flag3)
			{
				lastVoteStates[k] = new TwitchChatClientState.VoteState(pickableIndex, votes, state.SyncListVoteStates[k].newVotes);
			}
		}
	}

	public void DistributeVotes(int numberIntoPartyBox)
	{
		StartCoroutine(DistributeVotesCoroutine(numberIntoPartyBox));
	}

	private IEnumerator DistributeVotesCoroutine(int numberIntoPartyBox)
	{
		canvas.sortingLayerName = "Background 4";
		canvas.sortingOrder = -200;
		animatingDontUpdateContent = true;
		if (voteDisplayPanels[0].lastVoteCount != 0)
		{
			AkSoundEngine.PostEvent("UI_Twitch_Added_To_Party_Box", base.gameObject);
		}
		for (int i = voteDisplayPanels.Count - 1; i >= 0; i--)
		{
			voteDisplayPanels[i].SetUILayer("Background 4", -190);
			if (i < numberIntoPartyBox)
			{
				voteDisplayPanels[i].AnimateToCenter();
			}
			else
			{
				voteDisplayPanels[i].AnimateOffScreen();
			}
			yield return new WaitForSeconds(0.05f);
		}
		yield return new WaitForSeconds(2f);
		TwitchChatController.instance.EndAnimation();
		yield return new WaitForSeconds(3f);
		animatingDontUpdateContent = false;
		canvas.sortingLayerName = "UI 3";
		canvas.sortingOrder = 200;
		for (int num = voteDisplayPanels.Count - 1; num >= 0; num--)
		{
		}
	}

	public void OnLocalizationLanguageChange()
	{
		foreach (TwitchVoteDisplayLogic voteDisplayPanel in voteDisplayPanels)
		{
			voteDisplayPanel.UpdateLocalizedItemName();
		}
		UpdateNumVotes();
	}

	private void UpdateNumVotes()
	{
		numberOfVotesText.text = numberOfVotes + " " + ((numberOfVotes == 1) ? ScriptLocalization.Twitch_Voting.Num_Votes_Singular : ScriptLocalization.Twitch_Voting.Num_Votes);
	}
}
