using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class LeaderboardDisplay : MonoBehaviour
{

    [SerializeField] private Transform container;
    [SerializeField] private Transform entry;

    private const int Height = 85; //75 true
    private const int Offset = 370;

    private List<LeaderboardEntry> highscoreEntries;
    private List<Transform> highscoreEntryTransformList;

    private void Awake()
    {
        entry.gameObject.SetActive(false);
    }

    private void Start()
    {
        GameManager.instance.leaderboard.SaveEntry("ZBH", 1000);

        highscoreEntries = GameManager.instance.leaderboard.GetLeaderboardEntries();
        
        highscoreEntryTransformList = new List<Transform>();
        foreach (var highscoreEntry in highscoreEntries)
        {
            CreateHighscoreEntryTransform(highscoreEntry, container, highscoreEntryTransformList);
        }
    }

    private void CreateHighscoreEntryTransform(LeaderboardEntry highscoreEntry, Transform container, List<Transform> transformList)
    {
        Transform entryTransform = Instantiate(entry, container);
        RectTransform rectTransform = entryTransform.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, (-Height * transformList.Count) + Offset);
        entryTransform.gameObject.SetActive(true);
            
        int rank = transformList.Count + 1;
        string rankString;

        switch (rank)
        {
            default: rankString = rank + "th"; break;
            case 1: rankString = "1st"; break;
            case 2: rankString = "2nd"; break;
            case 3: rankString = "3rd"; break;
        }
        
          
        entryTransform.Find("Ranking").GetComponent<TextMeshProUGUI>().text = rankString;
        entryTransform.Find("Name").GetComponent<TextMeshProUGUI>().text = highscoreEntry.name;
        entryTransform.Find("Score").GetComponent<TextMeshProUGUI>().text = highscoreEntry.score.ToString("N0");
        
        transformList.Add(entryTransform);
    }
}
