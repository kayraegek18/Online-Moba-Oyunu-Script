using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class News : MonoBehaviour
{
    public GameObject newsObj;
    public Transform newsView;

	private void Update()
	{
        if (sureliNewsCoroutine == null)
            sureliNewsCoroutine = StartCoroutine(sureliNews());
	}

	Coroutine sureliNewsCoroutine = null;
    IEnumerator sureliNews()
	{
        GetNews();
        yield return new WaitForSeconds(5f);
        sureliNewsCoroutine = null;
	}

    public void GetNews()
    {
        GetTitleNewsRequest request = new GetTitleNewsRequest();
        request.Count = 3;

        PlayFabClientAPI.GetTitleNews(request, result =>
        {
            List<TitleNewsItem> news = result.News;

			foreach (Transform item in newsView)
			{
                Destroy(item.gameObject);
			}

            foreach (TitleNewsItem item in news)
            {
                GameObject p = Instantiate(newsObj, newsView);

                p.transform.GetChild(0).GetComponent<Text>().text = item.Title;
                p.transform.GetChild(1).GetComponent<Text>().text = item.Body;
                p.transform.GetChild(2).GetComponent<Text>().text = item.Timestamp.ToString();
            }

        }, OnError);
    }

	private void OnError(PlayFabError error)
	{
        Debug.LogError(error.GenerateErrorReport());
	}
}

/*
* Copyright © Gameiva 2021 - 2022
*/
