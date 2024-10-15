using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class LeaderboardDisplay : MonoBehaviour
{
    [NonSerialized] public LeaderboardManager leaderboard;

    [SerializeField] private Transform container;
    [SerializeField] private Transform entry;

    private const int Height = 85;
    private const int Offset = 370;

    private List<LeaderboardEntry> highscoreEntries;
    private List<Transform> highscoreEntryTransformList;

    [SerializeField] private Transform inputNamePanel; // Panel holding all the input fields
    [SerializeField] private TextMeshProUGUI char1;
    [SerializeField] private TextMeshProUGUI char2;
    [SerializeField] private TextMeshProUGUI char3;

    public InputActionAsset playerInput;
    private InputAction submitAction;  // Submit Character
    private InputAction charUpAction;  // Go up in the alphabet
    private InputAction charDownAction;  // Go down in the alphabet

    private string playerInitials = "AAA"; // Stores player initials
    private int currentCharIndex = 0;      // Tracks the current character being edited (0 = char1, 1 = char2, 2 = char3)
    private char[] currentInitials = { 'A', 'A', 'A' };  // Stores the current character values

    private void Awake()
    {
        leaderboard = new LeaderboardManager();
        leaderboard.Awake();
    }

    private void Start()
    {
        // See if score qualifies for leaderboard
        if (leaderboard.DoesScoreQualify(GameManager.instance.score))
        {
            inputNamePanel.gameObject.SetActive(true);
            SetupInputActions();
            UpdateInitialsUI();  // Display default initials (AAA) at the start
        }
        else
        {
            inputNamePanel.gameObject.SetActive(false);
            
            highscoreEntries = leaderboard.GetLeaderboardEntries();
            highscoreEntryTransformList = new List<Transform>();

            foreach (var highscoreEntry in highscoreEntries)
            {
                CreateHighscoreEntryTransform(highscoreEntry, container, highscoreEntryTransformList);
            }
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

    private void SetupInputActions()
    {
        submitAction = playerInput.FindAction("Enter");
        charUpAction = playerInput.FindAction("Up");
        charDownAction = playerInput.FindAction("Down");

        // Bind actions
        submitAction.performed += _ => ConfirmCharacter();
        charUpAction.performed += _ => NavigateCharacter(-1);   // Go to the next character
        charDownAction.performed += _ => NavigateCharacter(1); // Go to the previous character

        submitAction.Enable();
        charUpAction.Enable();
        charDownAction.Enable();
    }

    // Method to navigate the alphabet up or down
    private void NavigateCharacter(int direction)
    {
        // Modify current character by direction (+1 or -1)
        currentInitials[currentCharIndex] = (char)((currentInitials[currentCharIndex] - 'A' + direction + 26) % 26 + 'A');

        UpdateInitialsUI();
    }

    // Method to confirm the current character and move to the next one
    private void ConfirmCharacter()
    {
        // Move to the next character if any are remaining
        if (currentCharIndex < 2)
        {
            currentCharIndex++;
        }
        else
        {
            // Once all characters are confirmed, save the initials
            playerInitials = new string(currentInitials);
            SavePlayerInitials();
        }
        UpdateInitialsUI();
    }

    // Method to update the initials on the UI
    private void UpdateInitialsUI()
    {
        char1.text = currentInitials[0].ToString();
        char2.text = currentInitials[1].ToString();
        char3.text = currentInitials[2].ToString();

        // Optionally highlight the current character being modified
        char1.fontStyle = currentCharIndex == 0 ? FontStyles.Underline : FontStyles.Normal;
        char2.fontStyle = currentCharIndex == 1 ? FontStyles.Underline : FontStyles.Normal;
        char3.fontStyle = currentCharIndex == 2 ? FontStyles.Underline : FontStyles.Normal;
    }

    // Save the player initials to the leaderboard (you will implement this later)
    private void SavePlayerInitials()
    {
        leaderboard.SaveEntry(playerInitials, GameManager.instance.score);
        
        highscoreEntries = leaderboard.GetLeaderboardEntries();
        highscoreEntryTransformList = new List<Transform>();

        foreach (var highscoreEntry in highscoreEntries)
        {
            CreateHighscoreEntryTransform(highscoreEntry, container, highscoreEntryTransformList);
        }
        
        inputNamePanel.gameObject.SetActive(false);
    }
}