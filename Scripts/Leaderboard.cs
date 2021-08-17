using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class Leaderboard : MonoBehaviour
{
	public GameObject rowPrefab;
	public Transform rowsParent;

    public void SendLeaderboard(int score)
	{
		var request = new UpdatePlayerStatisticsRequest
		{
			Statistics = new List<StatisticUpdate>
			{
				new StatisticUpdate
				{
					StatisticName = "coin",
					Value = score
				}
			}
		};
		PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
	}

	public void GetLeaderboard()
	{
		var request = new GetLeaderboardRequest
		{
			StatisticName = "coin",
			StartPosition = 0,
			MaxResultsCount = 10
		};
		PlayFabClientAPI.GetLeaderboard(request, OnLeaderBoardGet, OnError);
	}

	private void OnLeaderBoardGet(GetLeaderboardResult result)
	{
		foreach (Transform item in rowsParent)
		{
			Destroy(item.gameObject);
		}

		foreach (var item in result.Leaderboard)
		{
			GameObject newGo = Instantiate(rowPrefab, rowsParent);
			Text[] texts = newGo.GetComponentsInChildren<Text>();
			texts[0].text = item.DisplayName;
			texts[1].text = (item.Position + 1).ToString();
			texts[2].text = item.StatValue.ToString();

			if (item.PlayFabId == PlayerPrefs.GetString("PLAYFAB_USER_ID"))
			{
				texts[0].color = Color.cyan;
				texts[1].color = Color.cyan;
				texts[2].color = Color.cyan;
			}

			Debug.Log($"{item.Position + 1}, {item.DisplayName}, {item.StatValue}");
		}
	}

	private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
	{
		Debug.Log("Successfull send leaderboard!");
	}

	private void OnError(PlayFabError error)
	{
		Debug.LogError(error.GenerateErrorReport());
	}
}

/*
* Copyright © Gameiva 2021 - 2022
*/
